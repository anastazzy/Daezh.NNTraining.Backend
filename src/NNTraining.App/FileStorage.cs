using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using NNTraining.Contracts;
using NNTraining.Contracts.Options;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;
using File = NNTraining.Domain.Models.File;

namespace NNTraining.App;

public class FileStorage: IFileStorage
{
    private readonly MinioClient _minio;
    private readonly NNTrainingDbContext _dbContext;

    public FileStorage(IOptions<MinioOptions> options, IServiceProvider serviceProvider)
    {
        _minio = new MinioClient()
            .WithEndpoint(options.Value.Endpoint)
            .WithCredentials(options.Value.AccessKey, options.Value.SecretKey);
        if (options.Value.Secure)
        {
            _minio.WithSSL();
        }
        _minio.Build();
        using var scope = serviceProvider.CreateScope();
        _dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
    }
    
    public async Task<Guid> UploadAsync(string fileName, string contentType, Stream fileStream,
        ModelType modelType, Guid idModel, FileType fileType)
    {
        var size = fileStream.Length;
        var newFileName = Guid.NewGuid();
        var file = new File
        {
            OriginalName = fileName,
            Extension = fileName[fileName.IndexOf('.')..], //get extension from string
            Size = size,
            FileType = fileType
        };
        _dbContext.Files.Add(file);
        var idFile = file.Id;
        _dbContext.ModelFiles.Add(new ModelFile()
        {
            FileId = idFile,
            ModelId = idModel
        });
        await _dbContext.SaveChangesAsync(); // where should be located await _dbContext?
        
        await CreateBucketAsync(modelType, fileType);      
        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(modelType.ToString())
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithObject(newFileName.ToString())
            .WithContentType(contentType));
        return newFileName;
    }

    public async Task<ObjectStat> GetAsync(Guid fileName, ModelType bucketName, string outputFileName = "temp.csv")
    {
        var file = new FileInfo(outputFileName);
        if (!file.Exists)
        {
            file.Create();
        }
        
        var result = await _minio.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName.ToString())
            .WithObject(fileName.ToString())
            .WithFile(outputFileName));
        
        if (result is null)
        {
            throw new ArgumentException("The file with current name ");
        }

        return result;
    }

    private async Task CreateBucketAsync(ModelType bucketName, FileType location)
    {
        try
        {
            var found = await _minio.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName.ToString()));
            if (!found)
            {
                await _minio.MakeBucketAsync(new MakeBucketArgs()
                    .WithBucket(bucketName.ToString())
                    .WithLocation(location.ToString()));
            }
        }
        catch (MinioException e)
        {
            throw new MinioException(e.Message);
        }
    }
}