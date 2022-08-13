using Microsoft.AspNetCore.Mvc;
using NNTraining.Contracts;
using NNTraining.Domain;

namespace NNTraining.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class Documents
{
    private readonly IDocumentService _service;

    public Documents(IDocumentService service)
    {
        _service = service;
    }
    [HttpPost]
    public async Task<string> UploadDocuments(IEnumerable<IFormFile> files)
    {
        var result = await  _service.UploadDocuments(files);
        return result;
    }
}