namespace NNTraining.Domain.Models;

public class ModelFieldNameType
{
    public Guid IdPair { get; set; }
    public Guid IdModel { get; set; }
    public Dictionary<string, Type>? PairFieldType { get; set; }
}