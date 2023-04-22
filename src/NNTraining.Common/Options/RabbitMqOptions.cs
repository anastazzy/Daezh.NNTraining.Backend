namespace NNTraining.Common.Options;

public class RabbitMqOptions
{
    public string? HostName { get; set; }
    public string? QueueToPredict { get; set; }
    public string? PredictionResult { get; set; }
    public string? SaveFileWithModel { get; set; }
    public string? QueueToTrain { get; set; }
    public string? QueueChangeModelStatus { get; set; }
    public string? Common { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}