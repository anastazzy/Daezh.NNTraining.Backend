using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using NNTraining.Contracts;
using NNTraining.Domain;
using NNTraining.Domain.Enums;
using NNTraining.Domain.Models;

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
    public async Task<string> UploadFile(IFormFile formFile, ModelType bucketName, Guid idModel, FileType type)
    {
        return await _storage.UploadAsync(
            formFile.FileName, formFile.ContentType, formFile.OpenReadStream(), bucketName, idModel, type);
    }
    
    [HttpGet]
    public async Task GetFile(string fileName,  ModelType bucketName)
    {
        await _storage.GetAsync(fileName, bucketName);
    }
}