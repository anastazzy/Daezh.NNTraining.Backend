namespace NNTraining.Contracts;

public interface ITrainedModel
{
    public object Predict(object data);
}