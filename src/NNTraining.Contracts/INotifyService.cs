using NNTraining.Domain.Enums;
using NNTraining.Domain.Models;

namespace NNTraining.Contracts;

public interface INotifyService
{
    Task<Model> UpdateStateAndNotify(Model model, ModelStatus newStatus);
}