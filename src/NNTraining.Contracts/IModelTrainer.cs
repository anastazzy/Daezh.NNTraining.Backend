namespace NNTraining.Contracts;

public interface IModelTrainer
{
    public Task<ITrainedModel> Train();
}