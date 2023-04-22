using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using NNTraining.Common.Options;

namespace NNTraining.Common;

public class CustomMinioClient : ICustomMinioClient
{
    private readonly MinioClient _minio;

    public CustomMinioClient(IOptions<MinioOptions> options)
    {
        _minio = new MinioClient()
            .WithEndpoint(options.Value.Endpoint)
            .WithCredentials(options.Value.AccessKey, options.Value.SecretKey);
        
        if (options.Value.Secure)
        {
            _minio.WithSSL();
        }
        _minio.Build();
    }
    
    public async Task UploadAsync(string bucket, string location, string contentType, Stream fileStream, 
        long size, string newFileName)
    {
        await CreateBucketAsync(bucket.ToLower(), location);      
        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithObject(newFileName)
            .WithContentType(contentType));
    }

    public async Task<ObjectStat> CopyStreamAsync(string fileName, string bucket, MemoryStream fileStream)
    {
        return await _minio.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithCallbackStream(stream => stream.CopyToAsync(fileStream)));
    }

    public async Task<ObjectStat> GetObjectAsync(string fileName, string bucket, string outputFileName)
    {
        return await _minio.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucket.ToLower())
                    .WithObject(fileName)
                    .WithFile(outputFileName));
    }

    private async Task CreateBucketAsync(string bucketName, string location)
    {
        var bucket = bucketName.ToLower();
        try
        {
            var found = await _minio.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket));
            if (!found)
            {
                await _minio.MakeBucketAsync(new MakeBucketArgs()
                    .WithBucket(bucket)
                    .WithLocation(location));
            }
        }
        catch (MinioException e)
        {
            throw new MinioException(e.Message);
        }
    }
}