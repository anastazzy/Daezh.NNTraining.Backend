using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using Microsoft.ML.Data;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Enums;
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

        var model = dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }
       
        if (model.ModelStatus != ModelStatus.ReadyToTraining)
        {
            throw new ArgumentException("The model not ready for training. You must specify the file - train set for training.");
        }
        
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

        var currentDirectory = Directory.GetCurrentDirectory();
        var oldFiles = Directory.GetFiles(currentDirectory);
        
        var tempFileForTrainModel = $"{model.Name}.csv";
        await _fileStorage.GetAsync(
            model.Parameters?.NameOfTrainSet!, 
            ModelType.DataPrediction,
            tempFileForTrainModel);
        
        //creation the trainer and train the model
        var factory = new ModelTrainerFactory
        {
            NameOfTrainSet = tempFileForTrainModel
        };
        
        var trainer = factory.CreateTrainer(model.Parameters);
        _trainedModel = trainer.Train(model.PairFieldType);
        await _modelStorage.SaveAsync(_trainedModel, model, data.ToSchema());
        
        model.ModelStatus = ModelStatus.Trained;
        await dbContext.SaveChangesAsync();
        
        var fileNamesAfterSave = Directory.GetFiles(currentDirectory);

        var filesToDelete = fileNamesAfterSave.Except(oldFiles).ToArray();
        foreach (var item in filesToDelete)
        {
            File.Delete(item);
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
    
    
    public Dictionary<string,string> GetSchemaOfModel(Guid id)
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

        string? targetFieldName;
        
        switch (model.Parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
            {
                targetFieldName = dataPredictionNnParameters.NameOfTargetColumn;
                break;
            }
            default: throw new Exception();
        }

        if (targetFieldName is not null)
        {
            fieldTypeField.Remove(targetFieldName);
        }

        return fieldTypeField;
    }
    
}
    