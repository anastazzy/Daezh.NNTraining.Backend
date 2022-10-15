using Innofactor.EfCoreJsonValueConverter;

namespace NNTraining.Domain.Models;

public class Model<T> where T: NNParameters
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }
    
    public ModelType ModelType { get; set; }
    
    public ModelStatus ModelStatus { get; set; }
    
    [JsonField]
    public T? Parameters { get; set; }
    
    [JsonField]
    public Dictionary<string, Type>? PairFieldType { get; set; }
}
