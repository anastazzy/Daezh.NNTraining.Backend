﻿using NNTraining.Common.Enums;

namespace NNTraining.TrainerWorker.Contracts;

public interface INotifyService
{
    Task UpdateStateAndNotify(ModelStatus newStatus, Guid modelId);
    
    Task SendResultOfTraining(object result, Guid modelId);
}