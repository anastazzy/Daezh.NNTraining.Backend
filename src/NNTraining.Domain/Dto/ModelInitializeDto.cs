using Microsoft.AspNetCore.Mvc;
using NNTraining.Common.Enums;
using NNTraining.Domain.Models;

namespace NNTraining.Domain.Dto;

public class ModelInitializeDto
{
    public string Name { get; set; }
    
    public ModelType ModelType { get; set; }
}