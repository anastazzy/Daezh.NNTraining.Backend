using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.ML;
using Microsoft.ML.Data;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.ServiceContracts;
using NNTraining.Contracts;
using NNTraining.Contracts.Resources;
using NNTraining.DataAccess;
using NNTraining.Domain;
using File = System.IO.File;

namespace NNTraining.App;

public class ModelInteractionService : IModelInteractionService
{
    private readonly IFileStorage _fileStorage;
    private readonly IStringLocalizer<EnumDescriptionResources> _stringLocalizer;
    private readonly IRabbitMqPublisherService _publisherService;
    private readonly IServiceProvider _serviceProvider;

    public ModelInteractionService(
        IServiceProvider serviceProvider, 
        IFileStorage fileStorage,
        IStringLocalizer<EnumDescriptionResources> stringLocalizer, IRabbitMqPublisherService _publisherService)
    {
        _serviceProvider = serviceProvider;
        _fileStorage = fileStorage;
        _stringLocalizer = stringLocalizer;
        this._publisherService = _publisherService;
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
        object fullParam;
        switch (parameters)
        {
            case DataPredictionNnParameters dataPredictionNnParameters:
            {
                fullParam = dataPredictionNnParameters;
                model.PairFieldType = await ModelHelper.CompletionTheDictionaryAsync(
                    stream,
                    dataPredictionNnParameters.Separators);
                break;
            }
            default: throw new Exception();
        }
        //событие обновления статуса надо закинуть в рэбит
        // model = await _notifyService.UpdateStateAndNotify(ModelStatus.StillTraining, model.Id);
        model.ModelStatus = ModelStatus.StillTraining;
        await dbContext.SaveChangesAsync();
        
        _publisherService.SendMessage(new ModelContract
        {
            Id = model.Id,
            Name = model.Name,
            ModelType = model.ModelType,
            ModelStatus = model.ModelStatus,
            Parameters = fullParam as NNParametersContract,
            PairFieldType = null
        }, Queues.ToTrain);
        
        //далее работу начинает воркер
        // //creation of dataViewSchema for save model in storage
        // var columns = ModelHelper.CreateTheTextLoaderColumn(model.PairFieldType);
        // var data = new DataViewSchema.Builder();
        // foreach (var item in columns)
        // {
        //     data.AddColumn(item.Name, new KeyDataViewType(typeof(UInt32),UInt32.MaxValue));
        // }
        //
        // var tempFileForTrainModel = $"{model.Name}.csv";
        // await _fileStorage.GetAsync(
        //     model.Parameters?.NameOfTrainSet!, 
        //     ModelType.DataPrediction,
        //     tempFileForTrainModel);
        //
        // //creation the trainer and train the model
        // var factory = new ModelTrainerFactory
        // {
        //     NameOfTrainSet = tempFileForTrainModel
        // };
        //
        // var trainer = factory.CreateTrainer(model.Parameters);
        // _trainedModel = trainer.Train(model.PairFieldType);
        // await _modelStorage.SaveAsync(_trainedModel, model, data.ToSchema());// это вызывается тоже в воркере
        //
        // await _notifyService.UpdateStateAndNotify(model, ModelStatus.Trained);
        
        // как понять, что модель натренирована? надо получить событие от воркера
        await dbContext.SaveChangesAsync();
        // }
        // catch (Exception e)
        // {
            // Console.WriteLine($"The erorr was happend in training proccess: {e}");
            // await _notifyService.UpdateStateAndNotify(model, ModelStatus.ErrorOfTrainingModel);
            await dbContext.SaveChangesAsync();
        // }
        // finally
        // {
            // var fileNamesAfterSave = Directory.GetFiles(currentDirectory);
            // var filesToDelete = fileNamesAfterSave.Except(oldFiles).ToArray();
            // foreach (var item in filesToDelete)
            // {
            //     File.Delete(item);
            // }
        // }
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
        var trainedModel = await _modelStorage.GetAsync(id, model.ModelType); // это вызывается тоже в воркере
        
        // после закидывания события в воркер то будет...
        //there dataset or object need for prediction
        var result = trainedModel.Predict(modelForPrediction);
        return result;
    }
    
    private async Task<GetAsyncContract> GetAsync(Guid id, ModelType bucketName)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        
        var model = dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model?.PairFieldType is null)
        {
            throw new ArgumentException("The model or it`s field name type was not found");
        }

        var modelFile = await dbContext.ModelFiles.FirstOrDefaultAsync(x =>
            x.ModelId == id && x.FileType == FileType.Model);
        if (modelFile is null)
        {
            throw new ArgumentException("The file with this model was not found");
        }

        var fileWithModel = await dbContext.Files.FirstOrDefaultAsync(x => x.Id == modelFile.FileId);
        // при сохранении модели сделать нормальным тип файла
        if (fileWithModel is null)
        {
            throw new ArgumentException("The file with this model was not found");
        }
        // далее тоже вызывается все в воркере
        
        // const string tempFileNameOfModel = "temp.zip";
        // await _storage.GetAsync(fileWithModel.GuidName, bucketName, tempFileNameOfModel);
        //
        // var trainedModel = _mlContext.Model.Load(tempFileNameOfModel, out var modelSchema);
        //
        // var type = ModelHelper.GetTypeOfCurrentFields(model.PairFieldType);
        //
        // switch (bucketName)
        // {
        //     case ModelType.DataPrediction:
        //     {
        //         var parameters = model.Parameters as DataPredictionNnParameters;
        //         if (parameters?.NameOfTargetColumn is null)
        //         {
        //             throw new ArgumentException("Error of conversion parameters");
        //         }
        //         return new DataPredictionTrainedModel(trainedModel, _mlContext, type, parameters.NameOfTargetColumn);
        //     }
        //     default: throw new Exception();
        // }
    }
}
    