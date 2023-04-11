using System.Text.Json;
using Microsoft.ML;

namespace NNTraining.TrainerWorker.Contracts;

public interface ITrainedModel
{
    object Predict (Dictionary<string, JsonElement> data);
    ITransformer GetTransformer();
}