using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using NNTraining.Contracts;
using NNTraining.Domain;
using NNTraining.Domain.Dto;

namespace NNTraining.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ModelInteractionController
{
    private readonly IModelInteractionService _modelService;

    public ModelInteractionController(IModelInteractionService modelService)
    {
        _modelService = modelService;
    }

    [HttpPost("train/{id}")]
    public void TrainModel([FromRoute] Guid id)
    {
        _modelService.Train(id);
    }
    
    [HttpPost("predict/{id}")]
    public Task<object> Predict([FromRoute] Guid id, [FromBody] Dictionary<string, JsonElement> objectToPredict)
    {
        return _modelService.Predict(id, objectToPredict);
    }
    
    [HttpGet("predict/{id}")]
    public Dictionary<string,string> GetSchema([FromRoute] Guid id)
    {
        return _modelService.GetSchemaOfModel(id);
    }
}