using Microsoft.AspNetCore.Mvc;
using NNTraining.Common;
using NNTraining.Contracts;
using NNTraining.Domain;
using NNTraining.Domain.Dto;

namespace NNTraining.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseModelService
{
    private readonly IBaseModelService _modelService;
    private readonly IRabbitMqService _rabbitMqService;

    public BaseModelService(IBaseModelService modelService, IRabbitMqService rabbitMqService)
    {
        _modelService = modelService;
        _rabbitMqService = rabbitMqService;
    }

    [HttpPost]
    public Task<Guid> InitModel(ModelInitializeDto modelDto)
    {
        return _modelService.InitializeModelAsync(modelDto);
    }
    
    [HttpPost("{id:Guid}/params")]
    public Task<Guid> FillingParamsModel([FromRoute] Guid id, [FromBody] DataPredictionNnParameters parameters)
    {
        return _modelService.FillingDataPredictionParamsAsync(new DataPredictionInputDto
        {
            Id = id,
            Parameters = parameters, 
        });
    }

    [HttpGet]
    public Task<ModelOutputDto[]> GetArrayOfModelsAsync()
    {
        return _modelService.GetListOfModelsAsync();
    }
    
    [HttpGet("{id:guid}")]
    public Task<ModelOutputDto?> GetModelAsync([FromRoute] Guid id)
    {
        return _modelService.GetModelAsync(id);
    } 
    
    [HttpPost("{id:Guid}/train-sets")]
    public Task<string> UploadTrainSet([FromRoute] Guid id, IFormFile trainSet)
    {
        return _modelService.UploadDatasetOfModelAsync(new UploadingDatasetModelDto
        {
            Id = id,
            UploadTrainSet = new FileInputDto
            {
                ContentType = trainSet.ContentType,
                FileName = trainSet.FileName,
                Stream = trainSet.OpenReadStream()
            },
        });
    }
    
    [HttpPatch("{id:Guid}/train-sets")]
    public Task<string?> SettingTrainSet([FromRoute] Guid id, [FromQuery] string name)
    {
        return _modelService.SetDatasetOfModelAsync(new ModelFileDto
        {
            Id = id,
            FileName = name,
        });
    }
    
    [HttpGet("{id:guid}/train-sets")]
    public FileOutputDto[] GetUploadedTrainSetsOfModel([FromRoute] Guid id)
    {
        return _modelService.GetUploadedTrainSetsForModel(id);
    }

    [HttpPut("{id:guid}")]
    public Task<bool> UpdateModelAsync(DataPredictionInputDto modelDto,[FromRoute] Guid id)
    {
        return _modelService.UpdateModelAsync(id, modelDto);
    }

    [HttpDelete("{id:guid}")]
    public Task<bool> DeleteModelAsync([FromRoute] Guid id)
    {
        return _modelService.DeleteModelAsync(id);
    }
    
    [HttpGet("types")]
    public IEnumerable<EnumOutputDto> GetModelTypes()
    {
        return _modelService.GetModelTypes();
    }
    
    [HttpGet("statuses")]
    public IEnumerable<EnumOutputDto> GetModelStatuses()
    {
        return _modelService.GetModelStatuses();
    }
    
    [HttpGet("schema/{id:guid}")]
    public Dictionary<string,string> GetSchema([FromRoute] Guid id)
    {
        return _modelService.GetSchemaOfModel(id);
    }
    
    [HttpPost("message")]
    public void Send()
    {
        _rabbitMqService.SendMessage("123-гори");
    }
}