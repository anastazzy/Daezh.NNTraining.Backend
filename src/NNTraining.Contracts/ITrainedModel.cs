using Microsoft.ML;

namespace NNTraining.Contracts;

public interface ITrainedModel
{
    object Predict(object data);
    ITransformer GetTransformer();
}