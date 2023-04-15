using System.Text;
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
        var factory = new ConnectionFactory { HostName =  _options.Value.HostName};
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _options.Value.PredictionResult,
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
            channel.BasicConsume(queue: _options.Value.PredictionResult,
                autoAck: true,
                consumer: consumer);
        } while (!stoppingToken.IsCancellationRequested);
        
        return Task.CompletedTask;
    }
    
    public async Task GetResult(PredictionResultContract contract)
    {
        // если будет какая-то история использования модели, то надо сохранять записи - вот зачем этот воркер
    }
}