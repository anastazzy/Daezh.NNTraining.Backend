namespace NNTraining.Contracts;

public interface IModelTrainer
{
    // public Task<ITrainedModel> Train();
    public ITrainedModel Train(Dictionary<string, Type> mapColumnNameColumnType);
}