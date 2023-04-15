using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NNTraining.Common;
using NNTraining.Common.Options;
using NNTraining.Common.QueueContracts;
using NNTraining.WebApi.DataAccess;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NNTraining.WebApi.Host.Workers;

public class TrainHostedListener : BackgroundService
{
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly ICustomMinioClient _minioClient;
    private readonly IRabbitMqPublisherService _publisherService;
    private readonly IServiceProvider _serviceProvider;

    public TrainHostedListener(IOptions<RabbitMqOptions> options, ICustomMinioClient minioClient, IRabbitMqPublisherService publisherService, 
        IServiceProvider serviceProvider)
    {
        _options = options;
        _minioClient = minioClient;
        _publisherService = publisherService;
        _serviceProvider = serviceProvider;
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

        channel.QueueDeclare(queue: _options.Value.QueueChangeModelStatus,
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
            channel.BasicConsume(queue: _options.Value.QueueChangeModelStatus,
                autoAck: true,
                consumer: consumer);
        } while (!stoppingToken.IsCancellationRequested);
        
        return Task.CompletedTask;
    }
    
    public async Task ChangeStateOfModel(ChangeModelStatusContract contract)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;

        var model = await dbContext.Models.FirstOrDefaultAsync(x => x.Id == contract.Id);

        if (model is null)
        {
            Console.WriteLine($" model with id={contract.Id} not found");
            return;
        }

        model.ModelStatus = contract.Status;
        
        await dbContext.SaveChangesAsync();
    }

}