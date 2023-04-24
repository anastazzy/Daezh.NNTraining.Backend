using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NNTraining.Common.Options;
using NNTraining.Common.QueueContracts;
using NNTraining.WebApi.DataAccess;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NNTraining.WebApi.Host.Workers;

public class ChangeStatusHostedListener : BackgroundService
{
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public ChangeStatusHostedListener(IOptions<RabbitMqOptions> options, IServiceProvider serviceProvider, ILogger<ChangeStatusHostedListener> logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
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
            var factory = new ConnectionFactory { HostName =  _options.Value.HostName};
            var connection = factory.CreateConnection();
            var model = connection.CreateModel();
        
            DeclareExchange(model, _options.Value.QueueChangeModelStatus);
        
            model.QueueDeclare(_options.Value.QueueChangeModelStatus, true, false, false);
            model.QueueBind(_options.Value.QueueChangeModelStatus, _options.Value.QueueChangeModelStatus, string.Empty);
        
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += async (_, ea) =>
            {
                var message = JsonSerializer.Deserialize<ChangeModelStatusContract>(ea.Body.Span)!;
                await ChangeStateOfModel(message);
            };
        
            model.BasicConsume(_options.Value.QueueChangeModelStatus, true, consumer);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e.Message);
        }
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
        model.UpdateDate = DateTimeOffset.Now;
        
        await dbContext.SaveChangesAsync();
    }
}