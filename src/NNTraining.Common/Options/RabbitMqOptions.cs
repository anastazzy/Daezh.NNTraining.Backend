namespace NNTraining.Common.Options;

public class RabbitMqOptions
{
    public string? HostName { get; set; }
    public string? TrainingQueueFromTrainer { get; set; }
    public string? TrainingQueueFromWebApi { get; set; }
    public string? TrainingQueueToTrain { get; set; }
    public string? QueueChangeModelStatus { get; set; }
    public string? Common { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}