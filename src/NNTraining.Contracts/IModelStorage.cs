using NNTraining.Domain.Models;using Microsoft.ML;
using NNTraining.Domain.Models;

namespace NNTraining.Contracts;

public interface IModelStorage
{
    public Task<Guid> SaveAsync(ITrainedModel trainedModel, Model model, DataViewSchema dataView);
    public Task<ITrainedModel> GetAsync(Guid id, ModelType modelType);
}