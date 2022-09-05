using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface IModelInteractionService
{
    public void Train(Guid id);
    public object Predict(Guid id, object modelForPrediction);
}