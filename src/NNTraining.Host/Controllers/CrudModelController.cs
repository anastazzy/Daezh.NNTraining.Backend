using Microsoft.AspNetCore.Mvc;
using NNTraining.Contracts;
using NNTraining.Domain;
using NNTraining.Domain.Dto;

namespace NNTraining.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class CrudModelController
{
    private readonly IBaseModelService _modelService;

    public CrudModelController(IBaseModelService modelService)
    {
        _modelService = modelService;
    }

    [HttpPost]
    public Task<Guid> SaveModelAsync(DataPredictionInputDto modelDto)
    {
        return _modelService.SaveDataPredictionModelAsync(modelDto);
    }
    
    // [HttpPost("createDataPrediction")]
    // public Task CreateDataPredictModel()
    // {
    //     return _modelService.CreateTheDataPrediction();
    // }
    
    // [HttpGet("dataPredictionSchema")]
    // public Dictionary<string, string> GetSchemaOfModel()
    // {
    //     return _modelService.GetSchemaOfModel();
    // }
    
    // [HttpPost("dataPredictionModel")]
    // public object UsingModel([FromBody] Dictionary<string,string> inputModelForUsing)
    // {
    //     return _modelService.UsingModel(inputModelForUsing);
    // }

    [HttpGet]
    public Task<ModelOutputDto[]> GetArrayOfModelsAsync()
    {
        return _modelService.GetListOfModelsAsync();
    }

    [HttpPut("id")]
    public Task<bool> UpdateModelAsync(DataPredictionInputDto modelDto, Guid id)
    {
        return _modelService.UpdateModelAsync(id, modelDto);
    }

    [HttpDelete("id")]
    public Task<bool> DeleteModelAsync(Guid id)
    {
        return _modelService.DeleteModelAsync(id);
    }
    
    [HttpGet("types")]
    public IEnumerable<TypeOutputDto> GetModelTypes()
    {
        return _modelService.GetModelTypes();
    }
}