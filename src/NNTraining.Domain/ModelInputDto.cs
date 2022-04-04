using System.Text.Json;
using System.Text.Json.Nodes;

namespace NNTraining.Domain;

public abstract class ModelInputDto<T> where T: NNParameters
{
    public string? Name { get; set; }
    public ModelType ModelType { get; set; }
    public ModelStatus ModelStatus { get; set; }
    
    public T Parameters { get; set; }
}