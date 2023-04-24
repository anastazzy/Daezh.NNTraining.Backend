using System.Text.Json;
using Microsoft.Extensions.Options;
using NNTraining.Common.Options;
using NNTraining.Common.ServiceContracts;
using NNTraining.WebApi.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NNTraining.WebApi.Host.Workers;

public class SaveModelHostedListener : BackgroundService
{
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger _logger;

    public SaveModelHostedListener(IOptions<RabbitMqOptions> options, IFileStorage fileStorage, ILogger<ChangeStatusHostedListener> logger)
    {
        _options = options;
        _fileStorage = fileStorage;
        _logger = logger;
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

    private void RunConsuming(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = _options.Value.HostName};
            var connection = factory.CreateConnection();
            var model = connection.CreateModel();
        
            DeclareExchange(model, _options.Value.SaveFileWithModel);
        
            model.QueueDeclare(_options.Value.SaveFileWithModel, true, false, false);
            model.QueueBind(_options.Value.SaveFileWithModel, _options.Value.SaveFileWithModel, string.Empty);
        
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (_, ea) =>
            {
                var message = JsonSerializer.Deserialize<SaveFileWithModelContract>(ea.Body.Span)!;
                SaveFileWithModel(message);
            };
        
            model.BasicConsume(_options.Value.QueueChangeModelStatus, true, consumer);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
        }
    }
    
    public void SaveFileWithModel(SaveFileWithModelContract contract)
    {
        _fileStorage.SaveModel(contract.ModelId, contract.FileIdInMinio, contract.Size);
    }
}