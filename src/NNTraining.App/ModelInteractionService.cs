using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;

namespace NNTraining.App;

public class ModelInteractionService: IModelInteractionService
{
    private readonly NNTrainingDbContext _dbContext;
    private ITrainedModel? _trainedModel = null;
    private readonly IModelStorage _storage;
    private readonly IModelTrainerFactory _modelTrainerFactory;

    public ModelInteractionService(NNTrainingDbContext dbContext, IModelStorage storage, IModelTrainerFactory modelTrainerFactory)
    {
        _dbContext = dbContext;
        _storage = storage;
        _modelTrainerFactory = modelTrainerFactory;
    }
    public async void Train(Guid id)
    {
        // 
        // 1 получить данные из бд для обучения модели
        // 2 получить из фабрики тренер
        // 3 сохранить обчуенную модель

        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }
        var factory = new ModelTrainerFactory();
        var trainer = factory.CreateTrainer(model.Parameters!);
        var trainedModel = await trainer.Train();//var trainedMdel

        model.DataViewSchema = null;//где сделать сохранение схемы и словаря в базу?
        var dictionary = model.PairFieldType; //null
        await _dbContext.SaveChangesAsync();
        var data = new DataViewSchema.Builder();
        
        foreach (var field in model.PairFieldType)
        {
            data.AddColumn(field.Key, new KeyDataViewType());
        }
        
        //мб сохранять саму схему, а не словарь 

        await _storage.SaveAsync(trainedModel, model, data.ToSchema());
        
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

    // public Dictionary<string,string> GetSchemaOfModel()
    // {
    //     return _creator.GetSchemaOfModel().ToDictionary(x => x.Item1, x => x.Item2.ToString());
    // }