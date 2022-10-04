using Microsoft.ML;

namespace NNTraining.Contracts;

public interface ITrainedModel
{
    public object Predict(object data);
    public ITransformer GetTransformer();
}