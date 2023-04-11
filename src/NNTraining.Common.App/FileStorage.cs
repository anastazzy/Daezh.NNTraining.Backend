using NNTraining.Common.Contracts;
using NNTraining.Common.Contracts.Options;
using File = NNTraining.Domain.Models.File;

namespace NNTraining.Common.App;

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

    private async Task<string?> SaveTrainSet(Guid modelId, string fileName, long size)
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
            ModelId = modelId,
            FileType = FileType.TrainSet
        });
        
        await dbContext.SaveChangesAsync();
        return file.GuidName;
    }
    
    private async Task<string?> SaveModel(Guid modelId, string fileName, long size)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        var file = new File
        {
            OriginalName = fileName,
            Extension = ".zip", 
            Size = size,
            GuidName =  Guid.NewGuid() + ".zip",
            FileType = FileType.Model
        };
        
        dbContext.Files.Add(file);
        var fileId = file.Id;

        var currentModelFile = dbContext.ModelFiles.FirstOrDefault(x => x.ModelId == modelId && x.FileType == FileType.Model);
        if (currentModelFile is null)
        {
            dbContext.ModelFiles.Add(new ModelFile()
            {
                FileId = fileId,
                ModelId = modelId,
                FileType = FileType.Model
            });
        }
        else
        {
            currentModelFile.FileId = fileId;
        }

        await dbContext.SaveChangesAsync();
        return file.GuidName;
    }

    public async Task<ObjectStat> GetAsync(string fileName, ModelType bucketName, string outputFileName = "temp.csv")
    {
        var bucket = bucketName.ToString().ToLower();
        var file = new FileInfo(outputFileName);
        if (!file.Exists)
        {
            file.Create().Close();
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
    
    public async Task<Stream> GetStreamAsync(string fileName, ModelType bucketName)
    {
        var fileStream = new MemoryStream(); 
        
        var bucket = bucketName.ToString().ToLower();
        
        var result = await _minio.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithCallbackStream(stream => stream.CopyToAsync(fileStream)));

        if (result is null)
        {
            throw new ArgumentException(nameof(result));
        }

        return fileStream;
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