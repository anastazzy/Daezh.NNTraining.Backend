namespace NNTraining.Domain;

public class Model
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public ModelType ModelType { get; set; }
    public ModelStatus ModelStatus { get; set; }
    
    public object? Parameters { get; set; }
}
