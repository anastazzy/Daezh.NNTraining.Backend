using NNTraining.Common.ServiceContracts;
using NNTraining.TrainerWorker.Contracts;

namespace NNTraining.TrainerWorker.App;

public class ModelTrainerFactory : IModelTrainerFactory
{
    public string NameOfTrainSet { get; set; }
    
    public IModelTrainer CreateTrainer(NNParameters parameters)
    {
        switch (parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
                return new DataPredictionModelTrainer(
                    NameOfTrainSet, 
                    dataPredictionNnParameters.NameOfTargetColumn!,
                    dataPredictionNnParameters.HasHeader,
                    dataPredictionNnParameters.Separators!);
            
            default: throw new Exception();
        }
    }
}