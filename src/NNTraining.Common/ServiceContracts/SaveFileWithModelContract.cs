namespace NNTraining.Common.ServiceContracts;

public class SaveFileWithModelContract
{
    public Guid ModelId { get; set; }
    public string? FileIdInMinio { get; set; }
    public long Size { get; set; }
}