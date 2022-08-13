using Microsoft.AspNetCore.Http;

namespace NNTraining.Contracts;

public interface IDocumentService
{
    Task<string> UploadDocuments(IEnumerable<IFormFile> files);
}