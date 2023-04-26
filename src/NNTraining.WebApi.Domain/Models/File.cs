using NNTraining.Common.Enums;

namespace NNTraining.WebApi.Domain.Models;

public class File
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    
    public string? OriginalName { get; set; }
    
    public string? GuidName { get; set; }
    
    public string? Extension { get; set; }
    
    public long Size { get; set; }
    
    public FileType FileType { get; set; }
    
    public Model Model { get; set; }
}