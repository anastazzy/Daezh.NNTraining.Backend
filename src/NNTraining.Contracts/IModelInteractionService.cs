using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface IModelInteractionService
{
    void Train(Guid id);
    Task<object> Predict(Guid id, object modelForPrediction);
    Dictionary<string, string> GetSchemaOfModel(Guid id);
}