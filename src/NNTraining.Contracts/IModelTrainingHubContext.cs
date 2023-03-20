namespace NNTraining.Contracts;

public interface IModelTrainingHubContext
{
    Task PullStatusOfTrainingAsync(int status, Guid idModel);
}