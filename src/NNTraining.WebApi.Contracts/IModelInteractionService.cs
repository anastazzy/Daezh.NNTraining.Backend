using System.Text.Json;

namespace NNTraining.WebApi.Contracts;

public interface IModelInteractionService
{
    void Train(Guid id);
    Task Predict(Guid id, Dictionary<string, JsonElement> modelForPrediction);
    // Dictionary<string, string> GetSchemaOfModel(Guid id);
}