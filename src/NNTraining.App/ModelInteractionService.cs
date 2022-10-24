using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using Microsoft.ML.Data;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Enums;
using NNTraining.Domain.Models;
using File = System.IO.File;

namespace NNTraining.App;

public class ModelInteractionService : IModelInteractionService
{
    private ITrainedModel? _trainedModel;
    private readonly IModelStorage _modelStorage;
    private readonly IFileStorage _fileStorage;
    private readonly IServiceProvider _serviceProvider;

    public ModelInteractionService(
        IServiceProvider serviceProvider, 
        IModelStorage modelStorage, 
        IFileStorage fileStorage)
    {
        _serviceProvider = serviceProvider;
        _modelStorage = modelStorage;
        _fileStorage = fileStorage;
    }

    public async void Train(Guid id)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        // 
        // 1 получить данные из бд для обучения модели
        // 1.1 дозаполнить данные, если они предоставлены
        // 2 получить из фабрики тренер
        // 3 сохранить обчуенную модель

        var model = dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }
       
        if (model.ModelStatus != ModelStatus.ReadyToTraining)
        {
            throw new ArgumentException("The model not ready for training. You must specify the file - train set for training.");
        }
        //для следующего пункта необходимо скачать а потом удалить с хоста файл - тренировочкный сет
        //попробовать получить стрим с минио
        var parameters = model.Parameters;
        if (parameters?.NameOfTrainSet is null) 
        {
            throw new ArgumentException("The model don`t has parameters.");
        }
        
        await using var stream = await _fileStorage.GetStreamAsync(parameters?.NameOfTrainSet!, model.ModelType);
            
        //save the field type and name to params of model
        switch (parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
            {
                model.PairFieldType = await ModelHelper.CompletionTheDictionaryAsync(
                    stream,
                    dataPredictionNnParameters.Separators);
                break;
            }
            default: throw new Exception();
        }
        await dbContext.SaveChangesAsync();

        //creation of dataViewSchema for save model in storage
        var columns = ModelHelper.CreateTheTextLoaderColumn(model.PairFieldType);
        var data = new DataViewSchema.Builder();
        foreach (var item in columns)
        {
            data.AddColumn(item.Name, new KeyDataViewType(typeof(UInt32),UInt32.MaxValue));
        }

        var fileNames = Directory.GetFiles(Directory.GetCurrentDirectory());
        
        var tempFileForTrainModel = $"{model.Name}.csv";
        var objectWithFileFromStorage = await _fileStorage.GetAsync(
            model.Parameters?.NameOfTrainSet!, 
            ModelType.DataPrediction,
            tempFileForTrainModel);
        
        //creation the trainer and train model
        var factory = new ModelTrainerFactory
        {
            NameOfTrainSet = tempFileForTrainModel
        };
        var trainer = factory.CreateTrainer(model.Parameters);
        
        
        _trainedModel = trainer.Train(model.PairFieldType);

        await _modelStorage.SaveAsync(_trainedModel, model, data.ToSchema());
        
        var fileNamesAfterSave = Directory.GetFiles(Directory.GetCurrentDirectory());

        var filesToDelete = fileNamesAfterSave.Except(fileNames).ToArray();
        for (int index = 0; index < filesToDelete.Length; index++)
        {
            File.Delete(filesToDelete[index]);
        }
    }

    public async Task<object> Predict(Guid id, Dictionary<string, JsonElement> modelForPrediction)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        
        var model = dbContext.Models.FirstOrDefault(x => x.Id == id);
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
    
    
    public Dictionary<string,string> GetSchemaOfModel(Guid id)//убрать из схемы целевое поле
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        
        var model = dbContext.Models.FirstOrDefault(x => x.Id == id);
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
    