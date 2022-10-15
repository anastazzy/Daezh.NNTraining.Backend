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
    private readonly IServiceProvider _serviceProvider;

    private const string Location = "datasets";
    //location = fileType
    //"dataprediction" - modelType - bucket

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
        
        _serviceProvider = serviceProvider;
    }
    
    public async Task<string> UploadAsync(string fileName, string contentType, Stream fileStream,
        ModelType modelType, Guid idModel, FileType fileType)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        var size = fileStream.Length;
        var newFileName = fileType switch 
        {
            FileType.TrainSet => await SaveTrainSet(idModel, fileName, size),
            FileType.Model => await SaveModel(idModel, fileName, size),
            _ => null,
        };
        if (newFileName is null)
        {
            return string.Empty;
        }

        var bucket = modelType.ToString().ToLower();
        
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        await CreateBucketAsync(modelType, FileType.PredictSet);      
        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithObject(newFileName)
            .WithContentType(contentType));
        
        return newFileName;
    }

    private async Task<string?> SaveTrainSet(Guid idModel, string fileName, long size)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        var file = new File
        {
            OriginalName = fileName,
            Extension = ".csv",
            Size = size,
            GuidName =   Guid.NewGuid() + ".csv",
        };
        dbContext.Files.Add(file);
        var idFile = file.Id;
        dbContext.ModelFiles.Add(new ModelFile()
        {
            FileId = idFile,
            ModelId = idModel,
            FileType = FileType.TrainSet
        });
        await dbContext.SaveChangesAsync();
        return file.GuidName;
    }
    
    private async Task<string?> SaveModel(Guid idModel, string fileName, long size)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        var file = new File
        {
            OriginalName = fileName,
            Extension = ".zip", 
            Size = size,
            GuidName =  Guid.NewGuid() + ".zip",
        };
        dbContext.Files.Add(file);
        var idFile = file.Id;
        dbContext.ModelFiles.Add(new ModelFile()
        {
            FileId = idFile,
            ModelId = idModel,
            FileType = FileType.Model
        });
        await dbContext.SaveChangesAsync();
        return file.GuidName;
    }

    public async Task<ObjectStat> GetAsync(string fileName, ModelType bucketName, string outputFileName = "temp.csv")
    {
        var bucket = bucketName.ToString().ToLower();
        var file = new FileInfo(outputFileName);
        if (!file.Exists)
        {
            file.Create();
        }
        
        var result = await _minio.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithFile(outputFileName));
        
        if (result is null)
        {
            throw new ArgumentException(nameof(result));
        }

        return result;
    }

    private async Task CreateBucketAsync(ModelType bucketName, FileType location)
    {
        var bucket = bucketName.ToString().ToLower();
        try
        {
            var found = await _minio.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket));
            if (!found)
            {
                await _minio.MakeBucketAsync(new MakeBucketArgs()
                        .WithBucket(bucket)
                    .WithLocation(location.ToString()));
            }
        }
        catch (MinioException e)
        {
            throw new MinioException(e.Message);
        }
    }
}