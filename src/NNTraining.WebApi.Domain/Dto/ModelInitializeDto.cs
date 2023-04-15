using NNTraining.Common.Enums;

namespace NNTraining.WebApi.Domain.Dto;

public class ModelInitializeDto
{
    public string Name { get; set; }
    
    public ModelType ModelType { get; set; }
}