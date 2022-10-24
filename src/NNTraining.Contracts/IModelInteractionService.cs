using System.Text.Json;
using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface IModelInteractionService
{
    void Train(Guid id);
    Task<object> Predict(Guid id, Dictionary<string, JsonElement> modelForPrediction);
    Dictionary<string, string> GetSchemaOfModel(Guid id);
}