using NNTraining.Common.Enums;

namespace NNTraining.TrainerWorker.Contracts;

public interface IModelTrainer
{
    ITrainedModel Train(object data);
}