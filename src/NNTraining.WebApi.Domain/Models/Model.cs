using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Innofactor.EfCoreJsonValueConverter;
using NNTraining.Common.Enums;
using NNTraining.WebApi.Domain.Tools;

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
