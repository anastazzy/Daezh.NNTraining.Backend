using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NNTraining.Common.Enums;
using NNTraining.WebApi.Contracts;
using NNTraining.WebApi.Contracts.Resources;
using NNTraining.WebApi.DataAccess;
using NNTraining.WebApi.Domain;
using NNTraining.WebApi.Domain.Dto;
using NNTraining.WebApi.Domain.Models;

namespace NNTraining.App;

public class BaseModelService : IBaseModelService
{
    private readonly NNTrainingDbContext _dbContext;
    private readonly IFileStorage _fileStorage;
    private readonly IStringLocalizer<EnumDescriptionResources> _localizer;

    public BaseModelService(
        NNTrainingDbContext dbContext, 
        IFileStorage fileStorage, 
        IStringLocalizer<EnumDescriptionResources> localizer)
    {
        _fileStorage = fileStorage;
        _localizer = localizer;
        _dbContext = dbContext;
    }

    public async Task<Guid> InitializeModelAsync(ModelInitializeDto modelInitializeDto)
    {
        var model = new Model
        {
            Name = modelInitializeDto.Name,
            ModelType = modelInitializeDto.ModelType,
            ModelStatus = ModelStatus.Initialized,
            CreationDate = DateTimeOffset.UtcNow,
            Priority = PriorityTraining.None,
            UpdateDate = DateTimeOffset.UtcNow
        };

        await _dbContext.Models.AddAsync(model);
        await _dbContext.SaveChangesAsync();
        return model.Id;
    }
    
    public async Task<string> UploadDatasetOfModelAsync(UploadingDatasetModelDto modelDto)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();
        var model = await _dbContext.Models
            .FirstOrDefaultAsync(x => x.Id == modelDto.Id);
        if (model is null)
        {
            throw new Exception("Model not found");
        }

        var file = modelDto.UploadTrainSet;
        if (file?.ContentType is null || file.Stream is null)
        {
            throw new ArgumentException("File is not reading");
        }
        
        if (!file.ContentType.Contains("csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The extension of file must be .csv");
        }
        
        var currentTime = DateTime.UtcNow.TimeOfDay;
        
        var guidNameTrainSet = await _fileStorage.UploadAsync(
            file.FileName + "_" + currentTime,
            file.ContentType,
            file.Stream,
            model.ModelType,
            model.Id,
            FileType.TrainSet);

        model.Parameters =  model.ModelType switch 
        {
            ModelType.DataPrediction => new DataPredictionNnParameters
            {
                NameOfTrainSet = guidNameTrainSet
            },
            _ => throw new Exception()
        };
        model.ModelStatus = ModelStatus.NeedAParameters;
        model.UpdateDate = DateTimeOffset.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return guidNameTrainSet;
    }

    public async Task<string?> SetDatasetOfModelAsync(ModelFileDto modelDto)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == modelDto.Id);
        if (model is null)
        {
            throw new Exception("Model not found");
        }

        var filesOfCurrentModel = _dbContext.ModelFiles
            .Where(x => x.ModelId == modelDto.Id && x.FileType == FileType.TrainSet)
            .Join(_dbContext.Files,
                modelFile => modelFile.FileId,
                file => file.Id,
                (modelFile, file) => new
                {
                    file.OriginalName,
                })
            .ToHashSet();

        if (!filesOfCurrentModel.Contains(new
            {
                OriginalName = modelDto.FileName
            }))
        {
            throw new Exception("The same file does not contains in file list of current model");
        }
        
        model.Parameters =  model.ModelType switch 
        {
            ModelType.DataPrediction => new DataPredictionNnParameters()
            {
                NameOfTrainSet = modelDto.FileName
            },
            _ => throw new Exception()
        };
        model.ModelStatus = ModelStatus.NeedAParameters;
        model.UpdateDate = DateTimeOffset.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return modelDto.FileName;
    }

    public FileOutputDto[] GetUploadedTrainSetsForModel(Guid modelId)
    { 
        return _dbContext.ModelFiles
            .Where(x => x.ModelId == modelId && x.FileType == FileType.TrainSet)
            .Join(_dbContext.Files, mf => mf.FileId, f => f.Id, (mf, f) => new FileOutputDto
            {
                ModelFileId = mf.Id,
                FileId = f.Id,
                FileName = f.OriginalName,
                FileNameInStorage = f.GuidName
            }).ToArray();
    }

    public async Task<Guid> FillingDataPredictionParamsAsync(DataPredictionInputDto modelDto)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();

        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == modelDto.Id);
        if (model is null)
        {
            throw new Exception("Model update ERROR");
        }
        //TODO: delete the nameOfTrainSet property from dto

        if (modelDto.Parameters is not null && model.ModelStatus >= ModelStatus.NeedAParameters)
        {
            var newParameters = new DataPredictionNnParameters
            {
                NameOfTrainSet = model.Parameters?.NameOfTrainSet?? modelDto.Parameters.NameOfTrainSet,
                NameOfTargetColumn = modelDto.Parameters.NameOfTargetColumn,
                HasHeader = modelDto.Parameters.HasHeader,
                Separators = modelDto.Parameters.Separators
            };

            model.Parameters = newParameters;
            model.ModelStatus = ModelStatus.ReadyToTraining;
            model.UpdateDate = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        return model.Id;
    }
    
    public async Task<ModelOutputDto[]> GetListOfModelsAsync()
    {
        var models = await _dbContext.Models.ToArrayAsync();
        return models.OrderBy(x => x.UpdateDate)
            .Select(model => new ModelOutputDto()
        {
            Id = model.Id,
            Name = model.Name,
            StatusName = _localizer[model.ModelStatus.ToString()],
            StatusId = (int)model.ModelStatus,
            TypeName = _localizer[model.ModelType.ToString()],
            Parameters = model.Parameters
        }).ToArray();
    }

    public async Task<ModelOutputDto?> GetModelAsync(Guid id)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        //TODO: Add the custom exceptions and middleware for catching them
        if (model is null)
        {
            throw new ArgumentException("Not found model");
        }
        return new ModelOutputDto
        {
            Id = model.Id,
            Name = model.Name,
            StatusName = _localizer[model.ModelStatus.ToString()],
            StatusId = (int)model.ModelStatus,
            TypeName = _localizer[model.ModelType.ToString()],
            Parameters = model.Parameters
        };
    }

    public async Task<bool> UpdateModelAsync(Guid id, DataPredictionInputDto modelDto)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null)
        {
            throw new Exception("Model update ERROR");
        }
        model.Parameters = modelDto.Parameters;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteModelAsync(Guid id)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null)
        {
            throw new Exception("Delete model ERROR");
        }
        _dbContext.Models.Remove(model);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public IEnumerable<EnumOutputDto> GetModelTypes()
    {
         return Enum.GetValues<ModelType>()
            .Select(x => new EnumOutputDto
            {
                Id = (int) x,
                Name = _localizer[x.ToString()],
            });
    }
    
    public IEnumerable<EnumOutputDto> GetModelStatuses()
    {
        return Enum.GetValues<ModelStatus>()
            .Select(x => new EnumOutputDto
            {
                Id = (int) x,
                Name = _localizer[x.ToString()],
            });
    }
    
    public Dictionary<string,string> GetSchemaOfModel(Guid id)
    {
        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model?.PairFieldType is null)
        {
            throw new ArgumentException("Not found");
        }

        var fieldTypeField = new Dictionary<string, string>();
        foreach (var (name, type) in model.PairFieldType)
        {
            fieldTypeField.Add(name, type.ToString());
        }

        string? targetFieldName;
        
        switch (model.Parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
            {
                targetFieldName = dataPredictionNnParameters.NameOfTargetColumn;
                break;
            }
            default: throw new Exception();
        }

        if (targetFieldName is not null)
        {
            fieldTypeField.Remove(targetFieldName);
        }

        return fieldTypeField;
    }
}