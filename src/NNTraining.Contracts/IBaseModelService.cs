using System.Text.Json;
using Minio.DataModel.Replication;
using NNTraining.Domain;
using NNTraining.Domain.Dto;
using NNTraining.Domain.Models;

namespace NNTraining.Contracts;

public interface IBaseModelService
{
    Task<Guid> InitializeModelAsync(ModelInitializeDto modelInitializeDto);
    
    Task<Guid> FillingDataPredictionParamsAsync(DataPredictionInputDto modelDto);
    
    Task<string> UploadDatasetOfModelAsync(UploadingDatasetModelDto modelDto);
    
    Task<ModelOutputDto[]> GetListOfModelsAsync();
    
    Task<ModelOutputDto?> GetModelAsync(Guid id);
    
    Task<bool> UpdateModelAsync(Guid id, DataPredictionInputDto modelDto);
    
    Task<bool> DeleteModelAsync(Guid id);
    
    IEnumerable<EnumOutputDto> GetModelTypes();
    
    IEnumerable<EnumOutputDto> GetModelStatuses();
}