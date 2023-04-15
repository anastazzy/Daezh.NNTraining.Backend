namespace NNTraining.WebApi.Domain.Dto;

public class UploadingDatasetModelDto
{
    public Guid Id { get; set; }
    
    public FileInputDto? UploadTrainSet { get; set; }
}