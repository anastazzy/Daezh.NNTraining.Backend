namespace NNTraining.Contracts;

public interface IModelStorage
{
    public Task<Guid> SaveAsync(ITrainedModel model);
    public Task<ITrainedModel> GetAsync(Guid id);
}