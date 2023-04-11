using NNTraining.Common.Enums;

namespace NNTraining.Common.ServiceContracts;

public class GetAsyncContract
{
    public ModelContract? Model { get; set; }
    public string? FileWithModelName { get; set; }
    public ModelType BucketName { get; set; }
}