using Microsoft.EntityFrameworkCore;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;

namespace NNTraining.Host;

public class CrudForModelService : ICrudForModelService
{
    private readonly NNTrainingDbContext _dbContext;

    public CrudForModelService(NNTrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> CreateModelAsync(DataPredictionInputDto modelDto)
    {
        var model = new Model
        {
            Name = modelDto.Name,
            ModelStatus = ModelStatus.Created,
            ModelType = modelDto.ModelType,
            Parameters = modelDto.Parameters
        };
        _dbContext.Models.Add(model);
        await _dbContext.SaveChangesAsync();
        return model.Id;
    }

    public async Task<ModelOutputDto[]> GetListOfModelsAsync()
    {
        var models = await _dbContext.Models.ToArrayAsync();
        return models.Select(model => new ModelOutputDto()
        {
            Id = model.Id,
            Name = model.Name,
            ModelStatus = model.ModelStatus,
            Parameters = model.Parameters,
        }).ToArray();
    }

    public async Task<bool> UpdateModelAsync(long id, DataPredictionInputDto modelDto)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null) throw new Exception("Model update ERROR");
        model.Name = modelDto.Name;
        model.Parameters = modelDto.Parameters;
        model.ModelType = modelDto.ModelType;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteModelAsync(long id)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null) throw new Exception("Delete model ERROR");
        _dbContext.Models.Remove(model);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public string[] GetModelTypes()
    {
        return Enum.GetNames(typeof(ModelType));
    }
}