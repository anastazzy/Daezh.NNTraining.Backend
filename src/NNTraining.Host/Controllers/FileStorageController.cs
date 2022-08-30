using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using NNTraining.Contracts;
using NNTraining.Domain;

namespace NNTraining.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class FileStorageController
{
    private readonly IFileStorage _storage;

    public FileStorageController(IFileStorage storage)
    {
        _storage = storage;
    }
    
    [HttpPost]
    public async Task<Guid> UploadFile(IFormFile formFile, string bucketName, Guid idModel)
    {
        return await _storage.UploadAsync(formFile.FileName, formFile.ContentType, formFile.OpenReadStream(), formFile.Length, bucketName, idModel);
    }
    
    [HttpGet]
    public async Task GetFile(string fileName,  string bucketName)
    {
        await _storage.GetAsync(fileName, bucketName);
    }
}