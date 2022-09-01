using Minio.DataModel;
using NNTraining.Domain.Models;

namespace NNTraining.Contracts;

public interface IFileStorage
{
    Task<Guid> UploadAsync(string fileName, string contentType, Stream fileStream,
        ModelType modelType, Guid idModel, FileType fileType);
    Task<ObjectStat> GetAsync(string fileName, ModelType bucketName);
}