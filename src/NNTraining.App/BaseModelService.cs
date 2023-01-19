using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NNTraining.Contracts;
using NNTraining.Contracts.Resources;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Dto;
using NNTraining.Domain.Enums;
using NNTraining.Domain.Models;

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
            ModelStatus = ModelStatus.Initialized
        };

        await _dbContext.Models.AddAsync(model);
        await _dbContext.SaveChangesAsync();
        return model.Id;
    }
    
    public async Task<string> UploadDatasetOfModelAsync(UploadingDatasetModelDto modelDto)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == modelDto.Id);
        if (model is null)
        {
            throw new Exception("Model not found");
        }

        var formFile = modelDto.UploadTrainSet;
        var contentType = formFile.ContentType;
        if (!contentType.Contains("csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The extension of file must be .csv");
        }

        var guidNameTrainSet = await _fileStorage.UploadAsync(
            formFile.Name,
            formFile.ContentType,
            formFile.OpenReadStream(),
            model.ModelType,
            model.Id,
            FileType.TrainSet);

        model.Parameters =  model.ModelType switch 
        {
            ModelType.DataPrediction => new DataPredictionNnParameters()
            {
                NameOfTrainSet = guidNameTrainSet
            },
            _ => throw new Exception()
        };
        model.ModelStatus = ModelStatus.NeedAParameters;
        
        _dbContext.Update(model);
        
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return guidNameTrainSet;
    }

    public async Task<Guid> FillingDataPredictionParamsAsync(DataPredictionInputDto modelDto)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();

        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == modelDto.Id);
        if (model is null)
        {
            throw new Exception("Model update ERROR");
        }
        //TODO: добавить проверку, что такое имя файла действительно существует

        if (modelDto.Parameters is not null && model.ModelStatus == ModelStatus.NeedAParameters)
        {
            var newParameters = new DataPredictionNnParameters
            {
                NameOfTrainSet = modelDto.Parameters.NameOfTrainSet,// TODO: может быть несколько сетов, спрашивать, но решить как-то с человекочитаемым названиями
                NameOfTargetColumn = modelDto.Parameters.NameOfTargetColumn,
                HasHeader = modelDto.Parameters.HasHeader,
                Separators = modelDto.Parameters.Separators
            };

            model.Parameters = newParameters;
            model.ModelStatus = ModelStatus.ReadyToTraining;
            _dbContext.Models.Update(model);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        return model.Id;// true or nothing
    }
    
    public async Task<ModelOutputDto[]> GetListOfModelsAsync()
    {
        var models = await _dbContext.Models.ToArrayAsync();
        return models.Select(model => new ModelOutputDto()
        {
            Id = model.Id,
            Name = model.Name,
            StatusName = _localizer[model.ModelStatus.ToString()],
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
}