using Microsoft.AspNetCore.SignalR;
using NNTraining.Contracts;

namespace NNTraining.App;

public class ModelTrainingHubContext : IModelTrainingHubContext
{
    private readonly IHubContext<ModelTrainingHub> _hub;

    public ModelTrainingHubContext(IHubContext<ModelTrainingHub> hub)
    {
        _hub = hub;
    }
    
    public Task PullStatusOfTrainingAsync(int status, Guid idModel)
    {
        return _hub.Clients.All.SendAsync("getLoadingElement", status, idModel);
    }
}