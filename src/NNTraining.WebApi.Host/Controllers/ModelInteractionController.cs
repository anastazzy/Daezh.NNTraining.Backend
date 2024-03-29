﻿using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NNTraining.WebApi.Contracts;

namespace NNTraining.WebApi.Host.Controllers;


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
    public Task Predict([FromRoute] Guid id, [FromBody] Dictionary<string, JsonElement> objectToPredict)
    {
        return _modelService.Predict(id, objectToPredict);
    }
}