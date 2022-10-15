using Microsoft.AspNetCore.Mvc;
using NNTraining.Contracts;
using NNTraining.Domain;
using NNTraining.Domain.Dto;

namespace NNTraining.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BaseModelService
{
    private readonly IBaseModelService _modelService;

    public BaseModelService(IBaseModelService modelService)
    {
        _modelService = modelService;
    }

    [HttpPost("init")]
    public Task<Guid> InitModel(ModelInitializeDto modelDto)
    {
        return _modelService.InitializeModelAsync(modelDto);
    }
    
    [HttpPost("filling-params")]
    public Task<Guid> FillingParamsModel(DataPredictionInputDto modelDto)
    {
        return _modelService.FillingDataPredictionParamsAsync(modelDto);
    }
    
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