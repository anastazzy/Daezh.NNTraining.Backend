﻿using NNTraining.Domain.Enums;
using NNTraining.Domain.Models;

namespace NNTraining.Domain.Dto;

public class ModelOutputDto
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }
    
    public ModelType ModelType { get; set; }
    
    public ModelStatus ModelStatus { get; set; }
    
    public string? NameTrainSet { get; set; }
    
    //public object? Parameters { get; set; }
}