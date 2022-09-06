using System.Text.Json;
using Minio.DataModel.Replication;
using NNTraining.Domain;
using NNTraining.Domain.Dto;

namespace NNTraining.Contracts;

public interface IBaseModelService
{
    Task<Guid> SaveDataPredictionModelAsync(DataPredictionInputDto modelDto);
    Task<bool> SetNameOfTrainSetAsync(Guid idModel, Guid idFile);
    Task<ModelOutputDto[]> GetListOfModelsAsync();
    Task<bool> UpdateModelAsync(Guid id, DataPredictionInputDto modelDto);
    Task<bool> DeleteModelAsync(Guid id);
    IEnumerable<TypeOutputDto> GetModelTypes();
}