using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.Options;
using NNTraining.Common.QueueContracts;
using NNTraining.TrainerWorker.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NNTraining.TrainerWorker.Host.Workers;

public class PredictHostedListener : BackgroundService
{
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly IModelStorage _modelStorage;
    private readonly INotifyService _notifyService;
    private readonly IRabbitMqPublisherService _publisherService;

    public PredictHostedListener(IOptions<RabbitMqOptions> options, IModelStorage modelStorage, 
        INotifyService notifyService, IRabbitMqPublisherService publisherService)
    {
        _options = options;
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
        var factory = new ConnectionFactory { HostName =  _options.Value.HostName};
        var connection = factory.CreateConnection();
        var model = connection.CreateModel();

        DeclareExchange(model, _options.Value.QueueToPredict);
        
        model.QueueDeclare(_options.Value.QueueToPredict, true, false, false);
        model.QueueBind(_options.Value.QueueToPredict, _options.Value.QueueToPredict, string.Empty);
        
        JsonSerializerOptions options = new();
        options.Converters.Add(new CustomModelParametersConverter());
        
        var consumer = new EventingBasicConsumer(model);
        consumer.Received += async (_, ea) =>
        {
            var message = JsonSerializer.Deserialize<PredictionContract>(ea.Body.Span, options)!;
            await Predict(message);
        };
        
        model.BasicConsume(_options.Value.QueueToPredict, true, consumer);
    }

    private async Task Predict(PredictionContract contract)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var oldFiles = Directory.GetFiles(currentDirectory);
        object result;

        try
        {
            await _notifyService.UpdateStateAndNotify(ModelStatus.StillPredict, contract.Model.Id);
            //getting trained model from a model storage
            var trainedModel = await _modelStorage.GetAsync(contract.Model, contract.FileWithModelName, contract.Model.ModelType);
        
            //there dataset or object need for prediction
            result = trainedModel.Predict(contract.ModelForPrediction);
            
            _publisherService.SendMessage(new PredictionResultContract
            {
                Id = contract.Model.Id,
                Result = result
            }, Queues.PredictionResult);
            
            await _notifyService.UpdateStateAndNotify(ModelStatus.Done, contract.Model.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine($"The erorr was happend in training proccess: {e}");
            await _notifyService.UpdateStateAndNotify(ModelStatus.ErrorOfTrainingModel, contract.Model.Id);
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

// SSO API -> login / register. Has separated database. After register publish info anout registered user
// after login return JWT token to the FE.

// Model Trainer API subsribes to new users and replicate them to its own database.
// Process requests from the specific users. Authentication works based on JWT. 
// Enqueue models for the training
// Also predictor API is here
// Subsribe trained models (to update status in DB and send notificaton)

// Model Trainer worker see waiting for training items in DB. Trains it. And notify users
// about training finished.
// 1. Get model where status = Enqueued Order By EnqueuedAt Limit 1
// 2. Train it
// 3. Update status = Trained
// 4. Send notification
// 5. Repeat 1 ot sleep 5 seconds