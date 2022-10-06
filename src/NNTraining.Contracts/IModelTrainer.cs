namespace NNTraining.Contracts;

public interface IModelTrainer
{
    ITrainedModel Train(Dictionary<string, Type> mapColumnNameColumnType);
}