using System.Text.Json.Serialization;

namespace NNTraining.Common.ServiceContracts;

public class ParametersContract<T> where T : NNParameters
{
    [JsonPropertyName("$type")]
    public Type Type { get; set; }
    
    [JsonPropertyName("$value")]
    public T Value { get; set; }
}