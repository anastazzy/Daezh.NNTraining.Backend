using NNTraining.Common.Enums;
using NNTraining.Common.ServiceContracts;
using NNTraining.TrainerWorker.Contracts;

namespace NNTraining.TrainerWorker.App;

public class NotifyService : INotifyService
{
    private readonly IModelTrainingHubContext _hubContext;

    public NotifyService(IModelTrainingHubContext hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task UpdateStateAndNotify(ModelStatus newStatus, Guid modelId)
    {
        // model.ModelStatus = newStatus;
        await _hubContext.PullStatusOfTrainingAsync((int)newStatus, modelId);
        // return model;
    }
}