using Microsoft.AspNetCore.Mvc;
using NNTraining.Domain.Enums;
using NNTraining.Domain.Models;

namespace NNTraining.Domain.Dto;

public class ModelInitializeDto
{
    public string Name { get; set; }
    
    public ModelType ModelType { get; set; }
}