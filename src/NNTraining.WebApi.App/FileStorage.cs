using Microsoft.Extensions.DependencyInjection;
using Minio.DataModel;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.WebApi.Contracts;
using NNTraining.WebApi.DataAccess;
using NNTraining.WebApi.Domain.Models;
using File = NNTraining.WebApi.Domain.Models.File;

namespace NNTraining.App;

public class FileStorage: IFileStorage
{
    private readonly ICustomMinioClient _customMinioClient;
    private readonly IServiceProvider _serviceProvider;

    public FileStorage(ICustomMinioClient customMinioClient, IServiceProvider serviceProvider)
    {
        _customMinioClient = customMinioClient;
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
        var location = fileType.ToString();

        await _customMinioClient.UploadAsync(bucket, location, contentType, fileStream, size, newFileName);
        
        return newFileName;
    }
    public async Task<ObjectStat> GetAsync(string fileName, ModelType bucketName, string outputFileName = "temp.csv")
    {
        var bucket = bucketName.ToString().ToLower();
        var file = new FileInfo(outputFileName);
        if (!file.Exists)
        {
            file.Create().Close();
        }

        var result = await _customMinioClient.GetObjectAsync(fileName, bucket, outputFileName);
        
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
        
        var result = await _customMinioClient.CopyStreamAsync(fileName, bucket, fileStream);

        if (result is null)
        {
            throw new ArgumentException(nameof(result));
        }

        return fileStream;
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
            GuidName = Guid.NewGuid() + ".csv",
            FileType = FileType.TrainSet
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
    
    public async Task<string?> SaveModel(Guid modelId, string fileName, long size)
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
}