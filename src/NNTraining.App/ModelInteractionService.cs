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
    private readonly IModelStorage _storage;

    public ModelInteractionService(NNTrainingDbContext dbContext, IModelStorage storage)
    {
        _dbContext = dbContext;
        _storage = storage;
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
                model.PairFieldType = await ModelHelper.CompletionTheDictionaryAsync(
                    dataPredictionNnParameters.NameOfTrainSet,
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
        var trainer = factory.CreateTrainer(model.Parameters!);
        var trainedModel = trainer.Train(model.PairFieldType);

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