using NNTraining.Common.Enums;

namespace NNTraining.Common.QueueContracts;

public class ChangeModelStatusContract
{
    public Guid Id { get; set; }
    public ModelStatus Status { get; set; }
}