using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Dto;
using NNTraining.Domain.Models;

namespace NNTraining.Host;

public class ModelFactory
{
    private readonly NNTrainingDbContext _dbContext;

    public ModelFactory(NNTrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task CreateModel(ModelInputDto dto)
    {
        var model = new Model
        {
            Name = dto.Name,
            ModelStatus = ModelStatus.Created,
            ModelType = dto.ModelType,
            Parameters = dto.Parameters
        };
        _dbContext.Models.Add(model);
        await _dbContext.SaveChangesAsync();
        
        switch (dto.Parameters)
        {
            case DataPredictionNNParameters dataPredictionNnParameters:
                return new CreatorOfModel(dataPredictionNnParameters.NameOfTrainSet,
                    dataPredictionNnParameters.NameOfTargetColumn);
            
            default: throw new Exception();
        }
    }
}