using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NNTraining.Common.Enums;
using NNTraining.Common.Options;
using RabbitMQ.Client;

namespace NNTraining.Common;

public class RabbitMqPublisherService : IRabbitMqPublisherService
{
    private readonly IOptions<RabbitMqOptions> _options;

    public RabbitMqPublisherService(IOptions<RabbitMqOptions> options)
    {
        _options = options;
    }

    public void SendMessage(object obj, Queues queue)
    {
        var queueName = queue switch
        {
            Queues.ChangeModelStatus => _options.Value.QueueChangeModelStatus,
            Queues.ToPredict => _options.Value.QueueToPredict,
            Queues.PredictionResult => _options.Value.PredictionResult,
            Queues.ToTrain => _options.Value.QueueToTrain,
            _ => _options.Value.Common
        };
        
        var factory = new ConnectionFactory { HostName =  _options.Value.HostName};
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        DeclareExchange(channel, queueName);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new CustomModelParametersConverter());

        channel.BasicPublish(queueName, string.Empty,
            body: JsonSerializer.SerializeToUtf8Bytes(obj, options));
    }

    private void DeclareExchange(IModel channel, string exchange)
    {
        channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
    }
}