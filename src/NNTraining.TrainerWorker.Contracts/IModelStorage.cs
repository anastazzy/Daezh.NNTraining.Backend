using Microsoft.ML;
using NNTraining.Common.Enums;
using NNTraining.Common.ServiceContracts;

namespace NNTraining.TrainerWorker.Contracts;

public interface IModelStorage
{
    Task<(string fileName, long size)> SaveAsync(ITrainedModel trainedModel, ModelContract model, DataViewSchema dataViewSchema);
    Task<ITrainedModel> GetAsync(ModelContract model, string fileWithModelName, ModelType bucketName);
}