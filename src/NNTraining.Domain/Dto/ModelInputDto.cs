using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NNTraining.Domain.Dto;

public abstract class ModelInputDto<T> where T: NNParameters
{
    public Guid Id { get; set; }
    
    public T? Parameters { get; set; }
}
