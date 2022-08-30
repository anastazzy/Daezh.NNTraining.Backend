using Minio.DataModel;
using NNTraining.Domain.Models;

namespace NNTraining.Contracts;

public interface IFileStorage
{
    Task<Guid> UploadAsync(string fileName, string contentType, Stream fileStream, long size, string bucketName, Guid idModel, FileType fileType);
    Task<ObjectStat> GetAsync(string fileName, string bucketName);
}