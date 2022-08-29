using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Dto;
using NNTraining.Domain.Models;

namespace NNTraining.Host;

public class CrudForModelService : ICrudForModelService
{
    private readonly NNTrainingDbContext _dbContext;

    private readonly IFileStorage _storage;

    private const string BucketDataPrediction = "dataprediction";
    // private CreatorOfModel _creator;

    public CrudForModelService(NNTrainingDbContext dbContext, IServiceProvider serviceProvider, IFileStorage storage)
    {
        _dbContext = dbContext;
        // _creator = (CreatorOfModel) serviceProvider.GetService(typeof(CreatorOfModel))!;
        _storage = storage;
    }

    public async Task<long> SaveDataPredictionModelAsync(DataPredictionInputDto modelDto)
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
    public async Task<bool> UpdateFileForModelAsync(long idModel, Guid idFile)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == idModel);
        if (model is null) throw new Exception("Model update ERROR");
        model.Parameters.NameOfTrainSet = idFile.ToString();
        model.ModelStatus = ModelStatus.ReadyToTraining;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    // private TParametrs GetParameters<TDto>() where TDto: ModelInputDto<TParametrs>
    // {
    //     return T switch
    //     {
    //         ModelType.DataPrediction => new DataPredictionNNParameters(),
    //     };
    // }
    
    //
    // public Task CreateTheDataPrediction()
    // {
    //     return _creator.Create();
    // }
    //


    // public async Task<IModelCreator> Create()
    // {
    //     
    //     var factory = new ModelFactory(_dbContext);
    //     var creator = await factory.CreateModel(modelDto);
    // }

    // public Dictionary<string,string> GetSchemaOfModel()
    // {
    //     return _creator.GetSchemaOfModel().ToDictionary(x => x.Item1, x => x.Item2.ToString());
    // }
    //
    // public object UsingModel(Dictionary<string,string> inputModelForUsing)
    // {
    //     return _creator.UsingModel(inputModelForUsing);
    // }
    

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

    public async Task<bool> UpdateModelAsync(long id, DataPredictionInputDto modelDto)
    {
        var model = await _dbContext.Models.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null) throw new Exception("Model update ERROR");
        model.Name = modelDto.Name;
        //model.Parameters = modelDto.Parameters;
        model.ModelType = ModelType.DataPrediction;
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