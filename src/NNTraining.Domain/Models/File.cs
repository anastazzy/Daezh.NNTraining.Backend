using NNTraining.Domain.Enums;

namespace NNTraining.Domain.Models;

public class File
{
    public Guid Id { get; set; }
    
    public string? OriginalName { get; set; }
    
    public string? GuidName { get; set; }
    
    public string? Extension { get; set; }
    
    public long Size { get; set; }
    
    public FileType FileType { get; set; }//add migrations
}