using Microsoft.EntityFrameworkCore;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Dto;
using NNTraining.Domain.Models;

namespace NNTraining.App;

public class BaseModelService : IBaseModelService
{
    private readonly NNTrainingDbContext _dbContext;

    public BaseModelService(NNTrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> SaveDataPredictionModelAsync(DataPredictionInputDto modelDto)
    {
        var modelParameters = new DataPredictionNnParameters
        {
            NameOfTrainSet = null,
            NameOfTargetColumn = modelDto.Parameters.NameOfTargetColumn,
            HasHeader = modelDto.Parameters.HasHeader,
            Separators = modelDto.Parameters.Separators
        };
        
        var model = new Model
        {
            Name = modelDto.Name,
            ModelStatus = ModelStatus.Created,
            ModelType = ModelType.DataPrediction,
            Parameters = modelParameters
        };
        _dbContext.Models.Add(model);
        await _dbContext.SaveChangesAsync();
        return model.Id;
    }
    
    public async Task<bool> SetNameOfTrainSetAsync(Guid idModel, Guid idFile)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == idModel);
        if (model?.Parameters is null)
        {
            throw new Exception("Model update ERROR");
        }
        model.Parameters.NameOfTrainSet = idFile.ToString();
        model.ModelStatus = ModelStatus.ReadyToTraining;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ModelOutputDto[]> GetListOfModelsAsync()
    {
        var models = await _dbContext.Models.ToArrayAsync();
        return models.Select(model => new ModelOutputDto()
        {
            Id = model.Id,
            Name = model.Name,
            ModelStatus = model.ModelStatus,
            //Parameters = model.Parameters,
        }).ToArray();
    }

    public async Task<bool> UpdateModelAsync(Guid id, DataPredictionInputDto modelDto)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null)
        {
            throw new Exception("Model update ERROR");
        }
        model.Name = modelDto.Name;
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