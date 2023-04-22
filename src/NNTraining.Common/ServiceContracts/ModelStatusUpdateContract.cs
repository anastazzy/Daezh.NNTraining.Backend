using NNTraining.Common.Enums;

namespace NNTraining.Common.ServiceContracts;

public class ModelStatusUpdateContract
{
    public Guid Id { get; set; }
    public ModelStatus Status { get; set; }
}