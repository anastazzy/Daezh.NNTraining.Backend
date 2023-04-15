using Minio.DataModel;

namespace NNTraining.Common;

public interface ICustomMinioClient
{
    Task UploadAsync(string bucket, string location, string contentType, Stream fileStream,
        long size, string newFileName);
    
    Task<ObjectStat> CopyStreamAsync(string fileName, string bucket, MemoryStream fileStream);

    Task<ObjectStat> GetObjectAsync(string fileName, string bucket, string outputFileName);
}