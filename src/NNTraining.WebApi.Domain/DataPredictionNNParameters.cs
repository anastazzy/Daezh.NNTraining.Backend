namespace NNTraining.WebApi.Domain;

public class DataPredictionNnParameters : NNParameters
{
    public string? NameOfTargetColumn { get; set; }
    public bool HasHeader { get; set; }
    public char[]? Separators { get; set; }
}