using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Models;

namespace NNTraining.Host;

public class ModelInteractionService: IModelInteractionService
{
    private readonly NNTrainingDbContext _dbContext;
    private ITrainedModel? _trainedModel = null;
    public ModelInteractionService(NNTrainingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async void Train(Guid id)
    {
        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }
        var factory = new ModelTrainerFactory();
        var trainer = factory.CreateTrainer(model.Parameters);
        _trainedModel = await trainer.Train();
        //save in Db or minio with modelStorage
        //fileNae = model.name
    }
    public object Predict(Guid id, object modelForPrediction)
    {
        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }

        return null;
        //     model.ModelType switch
        // {
        //     ModelType.DataPrediction => new DataPredictionTrainedModel(modelForPrediction)
        // };
        
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
}