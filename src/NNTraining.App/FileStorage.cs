using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using NNTraining.Contracts;
using NNTraining.Contracts.Options;

namespace NNTraining.Host;

public class FileStorage: IFileStorage
{
    private readonly MinioClient _minio;
    private const string BucketName = "dataprediction";
    private const string Location = "datasets";

    public FileStorage(IOptions<MinioOptions> options)
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
    
    public async Task UploadAsync(string fileName, string contentType, Stream fileStream, long size)
    {
        await CreateBucketAsync();      
        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(BucketName)
            //.WithFileName(fileName)
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithObject(fileName)
            .WithContentType(contentType));
    }

    private async Task CreateBucketAsync()
    {
        try
        {
            var found = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
            if (!found)
            {
                await _minio.MakeBucketAsync(new MakeBucketArgs()
                    .WithBucket(BucketName)
                    .WithLocation(Location));
            }
        }
        catch (MinioException e)
        {
            throw new MinioException(e.Message);
        }
    }
}