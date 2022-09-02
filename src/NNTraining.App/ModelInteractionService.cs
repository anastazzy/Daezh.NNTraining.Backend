using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;

namespace NNTraining.App;

public class ModelInteractionService: IModelInteractionService
{
    private readonly NNTrainingDbContext _dbContext;
    private ITrainedModel? _trainedModel = null;
    private readonly IModelStorage _storage;

    public ModelInteractionService(NNTrainingDbContext dbContext, IModelStorage storage)
    {
        _dbContext = dbContext;
        _storage = storage;
    }
    public async void Train(Guid id)
    {
        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }
        var dictionaryCreator = new DictionaryCreator();
        var factory = new ModelTrainerFactory();
        var trainer = factory.CreateTrainer(model.Parameters, dictionaryCreator);
        _trainedModel = await trainer.Train();

        var dictionary = dictionaryCreator.GetDictionary();
        await _dbContext.ModelFieldNameTypes.AddAsync(new ModelFieldNameType()
        {
            IdModel = id,
            PairFieldType = dictionary
        });

        await _storage.SaveAsync(_trainedModel, model);
        
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