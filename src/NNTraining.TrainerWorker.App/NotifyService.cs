using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.QueueContracts;
using NNTraining.TrainerWorker.Contracts;

namespace NNTraining.TrainerWorker.App;

public class NotifyService : INotifyService
{
    private readonly IModelTrainingHubContext _hubContext;
    private readonly IRabbitMqPublisherService _publisherService;

    public NotifyService(IModelTrainingHubContext hubContext, IRabbitMqPublisherService publisherService)
    {
        _hubContext = hubContext;
        _publisherService = publisherService;
    }

    public async Task UpdateStateAndNotify(ModelStatus newStatus, Guid modelId)
    {
        await _hubContext.PullStatusOfTrainingAsync((int)newStatus, modelId);
        _publisherService.SendMessage(new ChangeModelStatusContract
        {
            Id = modelId,
            Status = newStatus
        }, Queues.ChangeModelStatus);
    }
}