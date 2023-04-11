using NNTraining.Common.ServiceContracts;

namespace NNTraining.TrainerWorker.Contracts;

public interface IModelTrainerFactory
{
    IModelTrainer CreateTrainer(NNParametersContract parameters);
}