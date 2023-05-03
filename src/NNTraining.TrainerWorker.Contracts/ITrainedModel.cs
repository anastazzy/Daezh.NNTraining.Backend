using System.Text.Json;
using Microsoft.ML;

namespace NNTraining.TrainerWorker.Contracts;

public interface ITrainedModel
{
    object Predict (object data);
    ITransformer GetTransformer();
}