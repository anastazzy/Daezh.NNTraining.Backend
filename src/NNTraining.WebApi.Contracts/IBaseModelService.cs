using System.Text.Json;
using Minio.DataModel.Replication;
using NNTraining.WebApi.Domain;
using NNTraining.WebApi.Domain.Dto;
using NNTraining.WebApi.Domain.Models;

namespace NNTraining.WebApi.Contracts;

public interface IBaseModelService
{
    Task<Guid> InitializeModelAsync(ModelInitializeDto modelInitializeDto);
    
    Task<Guid> FillingDataPredictionParamsAsync(DataPredictionInputDto modelDto);
    
    Task<string> UploadDatasetOfModelAsync(UploadingDatasetModelDto modelDto);
    
    Task<string?> SetDatasetOfModelAsync(ModelFileDto modelDto);
    
    Task<ModelOutputDto[]> GetListOfModelsAsync();
    
    Task<ModelOutputDto?> GetModelAsync(Guid id);
    
    Task<bool> UpdateModelAsync(Guid id, DataPredictionInputDto modelDto);
    
    Task<bool> DeleteModelAsync(Guid id);
    
    IEnumerable<EnumOutputDto> GetModelTypes();
    
    IEnumerable<EnumOutputDto> GetModelStatuses();

    FileOutputDto[] GetUploadedTrainSetsForModel(Guid modelId);
    
    Dictionary<string, string> GetSchemaOfModel(Guid id);
}