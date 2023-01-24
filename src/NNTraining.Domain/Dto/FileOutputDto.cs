namespace NNTraining.Domain.Dto;

public class FileOutputDto
{
    public Guid ModelFileId { get; set; }
    
    public Guid FileId { get; set; }
    
    public string? FileName { get; set; }
    
    public string? FileNameInStorage { get; set; }
}