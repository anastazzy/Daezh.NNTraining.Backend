namespace NNTraining.TrainerWorker.Contracts;

public interface IModelTrainingHubContext
{
    Task PullStatusOfTrainingAsync(int status, Guid idModel);
}