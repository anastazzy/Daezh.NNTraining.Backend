using Microsoft.AspNetCore.Mvc;
using NNTraining.Contracts;
using NNTraining.Domain;

namespace NNTraining.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class CrudModelController
{
    private readonly ICrudForModelService _modelService;

    public CrudModelController(ICrudForModelService modelService)
    {
        _modelService = modelService;
    }

    [HttpPost]
    public Task<long> CreateModelAsync(DataPredictionInputDto modelDto)
    {
        return _modelService.CreateModelAsync(modelDto);
    }
    
    [HttpPost("createDataPrediction")]
    public Task CreateDataPredictModel()
    {
        return _modelService.CreateTheDataPrediction();
    }
    
    [HttpGet("dataPredictionSchema")]
    public Dictionary<string, string> GetSchemaOfModel()
    {
        return _modelService.GetSchemaOfModel();
    }
    
    [HttpPost("dataPredictionModel")]
    public float UsingModel([FromBody] object inputModelForUsing)
    {
        return _modelService.UsingModel(inputModelForUsing.ToString());
    }

    [HttpGet]
    public Task<ModelOutputDto[]> GetArrayOfModelsAsync()
    {
        return _modelService.GetListOfModelsAsync();
    }

    [HttpPut("id")]
    public Task<bool> UpdateModelAsync(DataPredictionInputDto modelDto, long id)
    {
        return _modelService.UpdateModelAsync(id, modelDto);
    }

    [HttpDelete("id")]
    public Task<bool> DeleteModelAsync(long id)
    {
        return _modelService.DeleteModelAsync(id);
    }
    
    [HttpGet("types")]
    public IEnumerable<TypeOutputDto> GetModelTypes()
    {
        return _modelService.GetModelTypes();
    }
}