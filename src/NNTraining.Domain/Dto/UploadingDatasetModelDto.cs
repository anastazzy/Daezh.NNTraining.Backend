using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NNTraining.Domain.Dto;

public class UploadingDatasetModelDto
{
    public Guid Id { get; set; }
    
    public FileInputDto? UploadTrainSet { get; set; }
}