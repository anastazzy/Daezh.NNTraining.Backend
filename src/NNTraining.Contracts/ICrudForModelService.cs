using System.Text.Json;
using Minio.DataModel.Replication;
using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface ICrudForModelService
{
    public Task<long> CreateModelAsync(DataPredictionInputDto modelDto);
    public Task CreateTheDataPrediction();
    public Dictionary<string,string> GetSchemaOfModel();
    public float UsingModel(Dictionary<string,string> inputModelForUsing);
    public Task<ModelOutputDto[]> GetListOfModelsAsync();
    public Task<bool> UpdateModelAsync(long id, DataPredictionInputDto modelDto);
    public Task<bool> DeleteModelAsync(long id);
    public IEnumerable<TypeOutputDto> GetModelTypes();
}