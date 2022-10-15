using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Models;

namespace NNTraining.App;

public class ModelInteractionService : IModelInteractionService
{
    private readonly NNTrainingDbContext _dbContext;
    private ITrainedModel? _trainedModel;
    private readonly IModelStorage _modelStorage;

    public ModelInteractionService(NNTrainingDbContext dbContext, IModelStorage modelStorage)
    {
        _dbContext = dbContext;
        _modelStorage = modelStorage;
    }

    public async void Train(Guid id)
    {
        // 
        // 1 получить данные из бд для обучения модели
        // 1.1 дозаполнить данные, если они предоставлены
        // 2 получить из фабрики тренер
        // 3 сохранить обчуенную модель

        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }
       
        if (model.ModelStatus != ModelStatus.ReadyToTraining)
        {
            throw new ArgumentException("The model not ready for training. You must specify the file - train set for training.");
        }

        //save the field type and name to params of model
        switch (model.Parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
            {
                model.PairFieldType = await ModelHelper.CompletionTheDictionaryAsync(
                    dataPredictionNnParameters.NameOfTrainSet.ToString(),
                    dataPredictionNnParameters.Separators);
                break;
            }
            default: throw new Exception();
        }
        await _dbContext.SaveChangesAsync();

        //creation of dataViewSchema for save model in storage
        var columns = ModelHelper.CreateTheTextLoaderColumn(model.PairFieldType);
        var data = new DataViewSchema.Builder();
        foreach (var item in columns)
        {
            data.AddColumn(item.Name, new KeyDataViewType(item.GetType(), item.KeyCount.Count!.Value));
        }
        
        //creation the trainer and train model
        var factory = new ModelTrainerFactory();
        var trainer = factory.CreateTrainer(model.Parameters);
        _trainedModel = trainer.Train(model.PairFieldType);

        await _modelStorage.SaveAsync(_trainedModel, model, data.ToSchema());
        
    }

    public async Task<object> Predict(Guid id, object modelForPrediction)
    {
        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }
        
        //getting trained model from a model storage

        var trainedModel = await _modelStorage.GetAsync(id, model.ModelType);
        
        //there dataset or object need for prediction
        var result = trainedModel.Predict(modelForPrediction);
        return result;
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

        return fieldTypeField;
    }
    
}
    