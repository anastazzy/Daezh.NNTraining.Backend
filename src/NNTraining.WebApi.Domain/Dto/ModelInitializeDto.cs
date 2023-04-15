using Microsoft.AspNetCore.Mvc;
using NNTraining.Common.Enums;
using NNTraining.WebApi.Domain.Models;

namespace NNTraining.WebApi.Domain.Dto;

public class ModelInitializeDto
{
    public string Name { get; set; }
    
    public ModelType ModelType { get; set; }
}