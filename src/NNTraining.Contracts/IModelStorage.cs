using NNTraining.Domain.Models;

namespace NNTraining.Contracts;

public interface IModelStorage
{
    public Task<Guid> SaveAsync(ITrainedModel trainedModel, Model model);
    public Task<ITrainedModel> GetAsync(Guid id);
}