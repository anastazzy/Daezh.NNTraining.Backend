namespace NNTraining.Common.ServiceContracts;

public class DataPredictionNnParametersContract : NNParametersContract
{
    public string? NameOfTargetColumn { get; set; }
    public bool HasHeader { get; set; }
    public char[]? Separators { get; set; }
}