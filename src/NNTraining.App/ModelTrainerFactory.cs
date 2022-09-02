using NNTraining.Contracts;
using NNTraining.Domain;

namespace NNTraining.App;

public class ModelTrainerFactory : IModelTrainerFactory
{
    public IModelTrainer CreateTrainer(NNParameters parameters, IDictionaryCreator dictionaryCreator)
    {
        switch (parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
                return new DataPredictionModelTrainer(
                    dataPredictionNnParameters.NameOfTrainSet!, 
                    dataPredictionNnParameters.NameOfTargetColumn!,
                    dataPredictionNnParameters.HasHeader,
                    dataPredictionNnParameters.Separators!, dictionaryCreator);
            
            default: throw new Exception();
        }
    }
}