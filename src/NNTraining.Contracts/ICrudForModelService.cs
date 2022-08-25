using System.Text.Json;
using Minio.DataModel.Replication;
using NNTraining.Domain;
using NNTraining.Domain.Dto;

namespace NNTraining.Contracts;

public interface ICrudForModelService
{
    public Task<long> SaveAsync(DataPredictionInputDto modelDto);
    public Task CreateTheDataPrediction();
    public Dictionary<string,string> GetSchemaOfModel();
    public object UsingModel(Dictionary<string,string> inputModelForUsing);
    public Task<ModelOutputDto[]> GetListOfModelsAsync();
    public Task<bool> UpdateModelAsync(long id, DataPredictionInputDto modelDto);
    public Task<bool> DeleteModelAsync(long id);
    public IEnumerable<TypeOutputDto> GetModelTypes();
}