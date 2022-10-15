using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NNTraining.Domain.Dto;

public class UploadingDatasetModelDto
{
    [FromRoute]
    public Guid Id { get; set; }
    
    [FromBody]
    public IFormFile? UploadTrainSet { get; set; }
}