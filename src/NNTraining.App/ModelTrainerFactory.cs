using NNTraining.Contracts;
using NNTraining.Domain;

namespace NNTraining.Host;

public class ModelTrainerFactory : IModelTrainerFactory
{
    public IModelTrainer CreateTrainer(NNParameters parameters)
    {
        switch (parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
                // return new DataPredictionModelTrainer(dataPredictionNnParameters.NameOfTrainSet,
                //     dataPredictionNnParameters.NameOfTargetColumn);
            
            default: throw new Exception();
        }
    }
}