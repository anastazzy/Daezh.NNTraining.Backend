using System.Text.Json;
using Minio.DataModel.Replication;
using NNTraining.Domain;
using NNTraining.Domain.Dto;

namespace NNTraining.Contracts;

public interface IBaseModelService
{
    public Task<Guid> SaveDataPredictionModelAsync(DataPredictionInputDto modelDto);
    public Task<ModelOutputDto[]> GetListOfModelsAsync();
    public Task<bool> UpdateModelAsync(Guid id, DataPredictionInputDto modelDto);
    public Task<bool> DeleteModelAsync(Guid id);
    public IEnumerable<TypeOutputDto> GetModelTypes();
}