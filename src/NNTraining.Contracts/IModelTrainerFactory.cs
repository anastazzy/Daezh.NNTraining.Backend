using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface IModelTrainerFactory
{
    public IModelTrainer CreateTrainer(NNParameters parameters, IDictionaryCreator dictionaryCreator);
}