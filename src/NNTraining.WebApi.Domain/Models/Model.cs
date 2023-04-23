﻿using System.ComponentModel.DataAnnotations.Schema;
using NNTraining.Common.Enums;

namespace NNTraining.WebApi.Domain.Models;

public class Model
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }
    
    public ModelType ModelType { get; set; }
    
    public ModelStatus ModelStatus { get; set; }
    
    [Column(TypeName = "jsonb")]
    public NNParameters? Parameters { get; set; }
    
    [Column(TypeName = "jsonb")]
    public Dictionary<string, Types>? PairFieldType { get; set; }
}