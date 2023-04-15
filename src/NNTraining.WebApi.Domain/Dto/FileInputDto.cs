namespace NNTraining.WebApi.Domain.Dto;

public class FileInputDto
{
    public string? ContentType { get; set; }
    
    public string? FileName { get; set; }
    
    public Stream? Stream { get; set; }
}