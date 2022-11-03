using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NNTraining.Contracts;
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

    public BaseModelService(NNTrainingDbContext dbContext, IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
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
            throw new Exception("Model update ERROR");
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

        if (modelDto.Parameters is not null && model.ModelStatus == ModelStatus.NeedAParameters)
        {
            var newParameters = new DataPredictionNnParameters
            {
                NameOfTrainSet = modelDto.Parameters.NameOfTrainSet,// может быть несколько сетов, спрашивать, но решить как-то с названиями
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
            ModelStatus = model.ModelStatus,
            ModelType = model.ModelType,
            NameTrainSet = model.Parameters?.NameOfTrainSet
        }).ToArray();
    }

    public async Task<Model?> GetModelAsync(Guid id)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        return model;
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

    public IEnumerable<TypeOutputDto> GetModelTypes()
    {
        return Enum.GetValues<ModelType>()
            .Select(x => new TypeOutputDto
            {
                Id = (int) x,
                Name = x.ToString(),
            });
    }
}