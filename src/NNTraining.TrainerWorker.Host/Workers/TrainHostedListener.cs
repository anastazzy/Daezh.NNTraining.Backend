using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.Options;
using NNTraining.Common.ServiceContracts;
using NNTraining.TrainerWorker.App;
using NNTraining.TrainerWorker.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NNTraining.TrainerWorker.Host.Workers;

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
        _ = Task.Factory.StartNew(() => RunConsuming(stoppingToken), TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }
    
    private void DeclareExchange(IModel channel, string exchange)
    {
        channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
    }

    public void RunConsuming(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = _options.Value.HostName};
        var connection = factory.CreateConnection();
        var model = connection.CreateModel();

        DeclareExchange(model, _options.Value.QueueToTrain);
        
        model.QueueDeclare(_options.Value.QueueToTrain, true, false, false);
        model.QueueBind(_options.Value.QueueToTrain, _options.Value.QueueToTrain, string.Empty);
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new CustomModelParametersConverter());
        
        var consumer = new EventingBasicConsumer(model);
        consumer.Received += async (_, ea) =>
        {
            var message = JsonSerializer.Deserialize<ModelContract>(ea.Body.Span, options)!;
            await Train(message);
        };
        
        model.BasicConsume(_options.Value.QueueToTrain, true, consumer);

    }

    private async Task Train(ModelContract model)
    {
        await _notifyService.UpdateStateAndNotify(ModelStatus.WaitingTraining, model.Id);
        var currentDirectory = Directory.GetCurrentDirectory();
        var oldFiles = Directory.GetFiles(currentDirectory);

        try
        {
            
            await _notifyService.UpdateStateAndNotify(ModelStatus.StillTraining, model.Id);
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
            var (fileWithModel, size) = await _modelStorage.SaveAsync(trainedModel, model, data.ToSchema());
            
            // в случае успеха закидывается на фронт смена статуса в хаб
            _publisherService.SendMessage(new SaveFileWithModelContract
            {
                ModelId = model.Id,
                FileIdInMinio = fileWithModel,
                Size = size
            }, Queues.ChangeModelStatus);
            
            _publisherService.SendMessage(new ModelStatusUpdateContract
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