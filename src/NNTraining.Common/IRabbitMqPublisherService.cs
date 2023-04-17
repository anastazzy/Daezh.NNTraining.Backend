using NNTraining.Common.Enums;

namespace NNTraining.Common;

public interface IRabbitMqPublisherService
{
    void SendMessage(object obj, Queues queue);
}