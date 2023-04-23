using Microsoft.AspNetCore.SignalR;
using NNTraining.TrainerWorker.Contracts;

namespace NNTraining.TrainerWorker.App;

public class ModelTrainingHubContext : IModelTrainingHubContext
{
    private readonly IHubContext<ModelTrainingHub> _hub;

    public ModelTrainingHubContext(IHubContext<ModelTrainingHub> hub)
    {
        _hub = hub;
    }
    
    public Task PullStatusOfTrainingAsync(int status, Guid modelId)
    {
        return _hub.Clients.All.SendAsync("getLoadingElement", status, modelId);
    }

    public Task SendResultOfPrediction(object result, Guid modelId)
    {
        return _hub.Clients.All.SendAsync("getPredictionResult", result, modelId);
    }
}