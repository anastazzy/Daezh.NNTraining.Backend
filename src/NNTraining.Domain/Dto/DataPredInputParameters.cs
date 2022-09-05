namespace NNTraining.Domain.Dto;

public class DataPredInputParameters: BaseInputParameters
{
    public string NameOfTargetColumn { get; set; }
    public bool HasHeader { get; set; }
    public char[] Separators { get; set; }
}