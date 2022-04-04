using System.Text.Json;
using System.Text.Json.Nodes;

namespace NNTraining.Domain;

public class ModelOutputDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public ModelType ModelType { get; set; }
    public ModelStatus ModelStatus { get; set; }
    
    public object? Parameters { get; set; }
}