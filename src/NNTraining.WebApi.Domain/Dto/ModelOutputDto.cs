using NNTraining.WebApi.Domain.Models;

namespace NNTraining.WebApi.Domain.Dto;

public class ModelOutputDto
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }

    public string? TypeName { get; set; }
    
    public string? StatusName { get; set; }
    
    public int StatusId { get; set; }
    
    public object? Parameters { get; set; }
}