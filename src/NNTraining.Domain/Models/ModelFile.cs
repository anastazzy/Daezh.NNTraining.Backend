﻿namespace NNTraining.Domain.Models;

public class ModelFile
{
    public Guid ModelId { get; set; }
    public Guid FileId { get; set; }
    
    public FileType FileType { get; set; }
}