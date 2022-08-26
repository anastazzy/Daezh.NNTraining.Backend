using NNTraining.Domain.Models;

namespace NNTraining.Domain.Dto;

public abstract class ModelInputDto<T> where T: NNParameters
{
    public string? Name { get; set; }
    public ModelType ModelType { get; set; }
    public ModelStatus ModelStatus { get; set; }
    
    public T Parameters { get; set; }
}

public class ModelInputDto
{
    public string? Name { get; set; }
    public ModelType ModelType { get; set; }
    public ModelStatus ModelStatus { get; set; }
    
    public NNParameters Parameters { get; set; }
}
