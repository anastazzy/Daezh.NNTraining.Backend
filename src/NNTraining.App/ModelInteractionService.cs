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
            throw new ApplicationException(
                "The model not ready for training. You must specify the file - train set for training.");
        }

        //save the field type and name to params of model
        switch (model.Parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
            {
                model.PairFieldType = await Helper.CompletionTheDictionaryAsync(
                    dataPredictionNnParameters.NameOfTrainSet,
                    dataPredictionNnParameters.Separators);
                break;
            }
            default: throw new Exception();
        }
        await _dbContext.SaveChangesAsync();

        //creation of dataViewSchema for save model in storage
        var columns = Helper.CreateTheTextLoaderColumn(model.PairFieldType);
        var data = new DataViewSchema.Builder();
        foreach (var item in columns)
        {
            data.AddColumn(item.Name, new KeyDataViewType(item.GetType(), item.KeyCount.Count!.Value));
        }

        //creation the trainer and train model
        var factory = new ModelTrainerFactory();
        var trainer = factory.CreateTrainer(model.Parameters!);
        _trainedModel = trainer.Train(model.PairFieldType);
        //save in Db or minio with modelStorage
        //fileNae = model.name
        
        await _modelStorage.SaveAsync(_trainedModel, model, data.ToSchema());
        
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