using NNTraining.Domain.Models;

namespace NNTraining.Domain.Dto;

public class ModelInitializeDto
{
    public string Name { get; set; }
    
    public ModelType ModelType { get; set; }
}