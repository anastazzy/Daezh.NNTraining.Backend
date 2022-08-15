namespace NNTraining.Contracts.Options;

public class MinioOptions
{
    public string AccessKey { get; init; }
    public string Endpoint { get; init; }
    public string SecretKey { get; init; }
    public bool Secure { get; set; }
}