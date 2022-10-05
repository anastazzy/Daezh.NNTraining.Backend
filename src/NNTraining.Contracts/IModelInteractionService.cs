using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface IModelInteractionService
{
    public void Train(Guid id);
    public Task<object> Predict(Guid id, object modelForPrediction);
}