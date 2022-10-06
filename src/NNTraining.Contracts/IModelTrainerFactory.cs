using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface IModelTrainerFactory
{
    IModelTrainer CreateTrainer(NNParameters parameters);
}