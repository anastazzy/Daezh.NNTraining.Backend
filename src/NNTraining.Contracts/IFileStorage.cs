using Minio.DataModel;

namespace NNTraining.Contracts;

public interface IFileStorage
{
    Task<Guid> UploadAsync(string fileName, string contentType, Stream fileStream, long size, string bucketName, Guid idModel);
    Task<ObjectStat> GetAsync(string fileName, string bucketName);
}