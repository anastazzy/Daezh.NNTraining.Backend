using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.Exceptions;
using NNTraining.Contracts;
using NNTraining.Contracts.Options;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;
using File = NNTraining.Domain.Models.File;

namespace NNTraining.Host;

public class FileStorage: IFileStorage
{
    private readonly MinioClient _minio;
    private readonly NNTrainingDbContext _dbContext;

    private const string Location = "datasets";
//"dataprediction"
    public FileStorage(IOptions<MinioOptions> options, NNTrainingDbContext dbContext)
    {
        _minio = new MinioClient()
            .WithEndpoint(options.Value.Endpoint)
            .WithCredentials(options.Value.AccessKey, options.Value.SecretKey);
        if (options.Value.Secure)
        {
            _minio.WithSSL();
        }
        _minio.Build();

        _dbContext = dbContext;
    }
    
    public async Task<Guid> UploadAsync(string fileName, string contentType, Stream fileStream, long size, string bucketName, Guid idModel, FileType fileType)
    {
        switch (fileType)
        {
            case FileType.TrainSet :
                SaveTrainSet(idModel, fileName, size);
                break;
            case FileType.SaveModelInStorage :
                SaveModel(idModel, fileName, size);
                break;
        };
        var newFileName = Guid.NewGuid();
        
        // Here?
        
        await CreateBucketAsync(bucketName);      
        await _minio.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithObject(newFileName.ToString())
            .WithContentType(contentType));
        await _dbContext.SaveChangesAsync(); // where should be located await _dbContext?
        return newFileName;
    }

    private void SaveTrainSet(Guid idModel, string fileName, long size)
    {
        var file = new File
        {
            OriginalName = fileName,
            Extension = fileName[fileName.IndexOf('.')..], //get extension from string
            Size = size
        };
        _dbContext.Files.Add(file);
        var idFile = file.Id;
        _dbContext.ModelFiles.Add(new ModelFile()
        {
            FileId = idFile,
            ModelId = idModel
        });
    }
    private void SaveModel(Guid idModel, string fileName, long size)
    {
        var file = new File
        {
            OriginalName = fileName,
            Extension = ".zip", //get extension from string
            Size = 0
        };
        _dbContext.Files.Add(file);
        var idFile = file.Id;
        _dbContext.ModelFiles.Add(new ModelFile()
        {
            FileId = idFile,
            ModelId = idModel
        });
    }

    public async Task<ObjectStat> GetAsync(string fileName, string bucketName)
    {
        var tempFileName = "temp.csv";
        
        var file = new FileInfo(tempFileName);
        if (!file.Exists)
        {
            file.Create();
        }
        
        var result = await _minio.GetObjectAsync(new GetObjectArgs().
            WithBucket(bucketName)
            .WithObject(fileName)
            .WithFile(tempFileName));
        
        if (result is null)
        {
            throw new Exception("null blin");
        }

        return result;
    }

    
    private async Task CreateBucketAsync(string bucketName)
    {
        try
        {
            var found = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!found)
            {
                await _minio.MakeBucketAsync(new MakeBucketArgs()
                    .WithBucket(bucketName)
                    .WithLocation(Location));
            }
        }
        catch (MinioException e)
        {
            throw new MinioException(e.Message);
        }
    }
}