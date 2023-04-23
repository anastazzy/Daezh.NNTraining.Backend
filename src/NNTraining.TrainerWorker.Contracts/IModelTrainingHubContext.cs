namespace NNTraining.TrainerWorker.Contracts;

public interface IModelTrainingHubContext
{
    Task PullStatusOfTrainingAsync(int status, Guid modelId);
    Task SendResultOfPrediction(object result, Guid modelId);
}