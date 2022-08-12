using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface ICrudForModelService
{
    public Task<long> CreateModelAsync(DataPredictionInputDto modelDto);
    public float CreateTheDataPrediction();
    public Task<ModelOutputDto[]> GetListOfModelsAsync();
    public Task<bool> UpdateModelAsync(long id, DataPredictionInputDto modelDto);
    public Task<bool> DeleteModelAsync(long id);
    public IEnumerable<TypeOutputDto> GetModelTypes();
}