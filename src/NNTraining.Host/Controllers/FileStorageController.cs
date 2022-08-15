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
    public async Task UploadFile(IFormFile formFile)
    {
        await _storage.UploadAsync(formFile.FileName, formFile.ContentType, formFile.OpenReadStream(), formFile.Length);
    }
    
    [HttpGet]
    public async Task GetFile(string fileName)
    {
        await _storage.GetAsync(fileName);
    }
}