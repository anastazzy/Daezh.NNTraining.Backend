using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NNTraining.Common.Options;
using NNTraining.Common.QueueContracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NNTraining.WebApi.Host.Workers;

public class PredictionResultHostedListener : BackgroundService
{
    private readonly IOptions<RabbitMqOptions> _options;

    public PredictionResultHostedListener(IOptions<RabbitMqOptions> options)
    {
        _options = options;
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
        var factory = new ConnectionFactory { HostName =  _options.Value.HostName};
        using var connection = factory.CreateConnection();
        var model = connection.CreateModel();

        DeclareExchange(model, _options.Value.PredictionResult);
        
        model.QueueDeclare(_options.Value.PredictionResult, true, false, false);
        model.QueueBind(_options.Value.PredictionResult, _options.Value.PredictionResult, string.Empty);
        
        var consumer = new EventingBasicConsumer(model);
        consumer.Received += async (_, ea) =>
        {
            var message = JsonSerializer.Deserialize<PredictionResultContract>(ea.Body.Span)!;
            await GetResult(message);
        };
        
        model.BasicConsume(_options.Value.PredictionResult, true, consumer);

    }

    public async Task GetResult(PredictionResultContract contract)
    {
        // если будет какая-то история использования модели, то надо сохранять записи - вот зачем этот воркер
    }
}