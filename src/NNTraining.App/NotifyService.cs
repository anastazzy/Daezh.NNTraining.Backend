using NNTraining.Contracts;
using NNTraining.Domain.Enums;
using NNTraining.Domain.Models;

namespace NNTraining.App;

public class NotifyService : INotifyService
{
    private readonly IModelTrainingHubContext _hubContext;

    public NotifyService(IModelTrainingHubContext hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<Model> UpdateStateAndNotify(Model model, ModelStatus newStatus)
    {
        model.ModelStatus = newStatus;
        await _hubContext.PullStatusOfTrainingAsync((int)newStatus, model.Id);
        return model;
    }
}