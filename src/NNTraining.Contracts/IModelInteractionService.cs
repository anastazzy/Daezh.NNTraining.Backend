using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface IModelInteractionService
{
    public void Train(long id, NNParameters parameters);
    public object Predict(object modelForPrediction);
}