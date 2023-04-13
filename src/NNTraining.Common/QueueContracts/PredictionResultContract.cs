namespace NNTraining.Common.QueueContracts;

public class PredictionResultContract
{
    public Guid Id { get; set; }
    public object? Result { get; set; }
}