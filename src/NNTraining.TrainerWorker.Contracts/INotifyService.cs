using NNTraining.Common.Enums;
using NNTraining.Common.ServiceContracts;

namespace NNTraining.TrainerWorker.Contracts;

public interface INotifyService
{
    Task UpdateStateAndNotify(ModelStatus newStatus, Guid modelId);
}