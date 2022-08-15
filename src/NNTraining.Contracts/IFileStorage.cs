using Microsoft.AspNetCore.Http;

namespace NNTraining.Contracts;

public interface IFileStorage
{
    Task UploadAsync(string fileName, string contentType, Stream fileStream, long size);
}