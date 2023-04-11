using Microsoft.AspNetCore.SignalR;

namespace NNTraining.TrainerWorker.App;

public class ModelTrainingHub : Hub
{
    public Task PullStatusOfTrainingAsync()
    {
        return Clients.Caller.SendAsync("getTrainStatus");
    }
}
