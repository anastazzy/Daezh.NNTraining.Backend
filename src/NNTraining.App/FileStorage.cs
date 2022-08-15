using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
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
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithObject(fileName)
            .WithContentType(contentType));
    }

    public async Task<ObjectStat> GetAsync(string fileName)
    {
        var tempFileName = "temp.csv";
        
        var file = new FileInfo(tempFileName);
        if (!file.Exists)
        {
            file.Create();
        }
        
        var result = await _minio.GetObjectAsync(new GetObjectArgs().
            WithBucket(BucketName)
            .WithObject(fileName)
            .WithFile(tempFileName));
        
        if (result is null)
        {
            throw new Exception("null blin");
        }

        return result;
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