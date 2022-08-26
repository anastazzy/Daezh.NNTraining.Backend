using NNTraining.Domain.Models;

namespace NNTraining.Domain.Dto;

// public abstract class ModelInputDto<T> where T: NNParameters
// {
//     public string? Name { get; set; }
//     // public ModelType ModelType { get; set; }
//     // public ModelStatus ModelStatus { get; set; }
//     
//     public T Parameters { get; set; }
// }

public abstract class ModelInputDto<T> where T: BaseInputParameters
{
    public string? Name { get; set; }
    
    public T Parameters { get; set; }
}
