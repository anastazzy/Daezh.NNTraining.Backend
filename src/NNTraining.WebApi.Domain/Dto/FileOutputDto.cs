namespace NNTraining.WebApi.Domain.Dto;

public class FileOutputDto
{
    public Guid Id { get; set; }

    public string? FileName { get; set; }
    
    public string? FileNameInStorage { get; set; }
}