using System.Text.Json.Serialization;
using NNTraining.Common.Enums;

namespace NNTraining.Common.ServiceContracts;

public class ModelContract
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }
    
    public ModelType ModelType { get; set; }
    
    public ModelStatus ModelStatus { get; set; }
    public NNParameters? Parameters { get; set; }
    public string? TypeParameters { get; set; }
    
    public Dictionary<string, Types>? PairFieldType { get; set; }
}
//
// public class ModelContract<T> where T: NNParameters
// {
//     public Guid Id { get; set; }
//     
//     public string? Name { get; set; }
//     
//     public ModelType ModelType { get; set; }
//     
//     public ModelStatus ModelStatus { get; set; }
//     public ParametersContract<T>? Parameters { get; set; }
//     
//     public Dictionary<string, Types>? PairFieldType { get; set; }
// }
