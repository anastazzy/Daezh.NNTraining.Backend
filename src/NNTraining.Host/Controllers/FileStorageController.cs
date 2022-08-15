using Microsoft.AspNetCore.Mvc;
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
    public async Task UploadDocuments(IFormFile formFile)
    {
        await _storage.UploadAsync(formFile.FileName, formFile.ContentType, formFile.OpenReadStream(), formFile.Length);
    }
}