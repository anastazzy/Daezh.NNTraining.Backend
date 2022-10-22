using NNTraining.Domain.Enums;

namespace NNTraining.Contracts;

public interface IModelTrainer
{
    ITrainedModel Train(Dictionary<string, Types> mapColumnNameColumnType);
}