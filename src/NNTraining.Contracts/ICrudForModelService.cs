using NNTraining.Domain;

namespace NNTraining.Contracts;

public interface ICrudForModelService
{
    public Task<long> CreateModel();
    public Task<List<Model>> GetListOfModels();
    public Task<bool> UpdateModel(long id);
    public Task<bool> DeleteModel(long id);
}