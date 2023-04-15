using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using Minio.DataModel;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.Options;
using NNTraining.Common.ServiceContracts;
using NNTraining.TrainerWorker.App;
using NNTraining.TrainerWorker.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NNTraining.TrainerWorker.Host;

public class TrainHostedListener : BackgroundService
{
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly ICustomMinioClient _minioClient;
    private readonly IModelStorage _modelStorage;
    private readonly INotifyService _notifyService;
    private readonly IRabbitMqPublisherService _publisherService;

    public TrainHostedListener(IOptions<RabbitMqOptions> options, ICustomMinioClient minioClient, IModelStorage modelStorage, 
        INotifyService notifyService, IRabbitMqPublisherService publisherService)
    {
        _options = options;
        _minioClient = minioClient;
        _modelStorage = modelStorage;
        _notifyService = notifyService;
        _publisherService = publisherService;
    }
    /// <summary>
    /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
    /// the lifetime of the long running operation(s) being performed.
    /// </summary>
    /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName =  _options.Value.HostName};
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _options.Value.QueueToTrain,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        
        var consumer = new EventingBasicConsumer(channel);
        
        do
        {
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                //здесь надо прокидывать их в predict метод
                Console.WriteLine($" [x] Received {message}");
            };
            channel.BasicConsume(queue: _options.Value.QueueToTrain,
                autoAck: true,
                consumer: consumer);
        } while (!stoppingToken.IsCancellationRequested);
        
        return Task.CompletedTask;
    }

    private async Task Train(ModelContract model)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var oldFiles = Directory.GetFiles(currentDirectory);

        try
        {
            //creation of dataViewSchema for save model in storage
            var columns = ModelHelper.CreateTheTextLoaderColumn(model.PairFieldType);
            var data = new DataViewSchema.Builder();
            foreach (var item in columns)
            {
                data.AddColumn(item.Name, new KeyDataViewType(typeof(UInt32),UInt32.MaxValue));
            }

            var tempFileForTrainModel = $"{model.Name}.csv";
            await _minioClient.GetObjectAsync( model.Parameters?.NameOfTrainSet!, 
                ModelType.DataPrediction.ToString(),
                tempFileForTrainModel);

            //creation the trainer and train the model
            var factory = new ModelTrainerFactory
            {
                NameOfTrainSet = tempFileForTrainModel
            };
        
            var trainer = factory.CreateTrainer(model.Parameters);
            var trainedModel = trainer.Train(model.PairFieldType);
            await _modelStorage.SaveAsync(trainedModel, model, data.ToSchema());
            
            // в случае успеха закидывается на фронт смена статуса в хаб
            _publisherService.SendMessage(new ModelStatusUpdateDto
            {
                Id = model.Id,
                Status = ModelStatus.Trained
            }, Queues.ChangeModelStatus);
            await _notifyService.UpdateStateAndNotify(ModelStatus.Trained, model.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine($"The erorr was happend in training proccess: {e}");
            await _notifyService.UpdateStateAndNotify(ModelStatus.ErrorOfTrainingModel, model.Id);
        }
        finally
        {
            var fileNamesAfterSave = Directory.GetFiles(currentDirectory);
            var filesToDelete = fileNamesAfterSave.Except(oldFiles).ToArray();
            foreach (var item in filesToDelete)
            {
                File.Delete(item);
            }
        }
    }
}