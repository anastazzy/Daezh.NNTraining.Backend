using Innofactor.EfCoreJsonValueConverter;

namespace NNTraining.Domain.Models;

public class Model
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public ModelType ModelType { get; set; }
    public ModelStatus ModelStatus { get; set; }
    [JsonField]
    public NNParameters? Parameters { get; set; }
    [JsonField]
    public Dictionary<string, Type>? PairFieldType { get; set; }
}
