using NNTraining.Domain.Models;using Microsoft.ML;
using NNTraining.Domain.Models;

namespace NNTraining.Contracts;

public interface IModelStorage
{
    Task<string> SaveAsync(ITrainedModel trainedModel, Model model, DataViewSchema dataView);
    Task<ITrainedModel> GetAsync(Guid id, ModelType modelType);
}