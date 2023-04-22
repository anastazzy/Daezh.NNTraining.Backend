using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.QueueContracts;
using NNTraining.Common.ServiceContracts;
using NNTraining.WebApi.Contracts;
using NNTraining.WebApi.Contracts.Resources;
using NNTraining.WebApi.DataAccess;
using NNParameters = NNTraining.Common.ServiceContracts.NNParameters;

namespace NNTraining.App;

public class ModelInteractionService : IModelInteractionService
{
    private readonly IFileStorage _fileStorage;
    private readonly IStringLocalizer<EnumDescriptionResources> _stringLocalizer;

    private readonly IWebAppPublisherService _publisherService;

    // private readonly IRabbitMqPublisherService _publisherService;
    private readonly IServiceProvider _serviceProvider;

    public ModelInteractionService(
        IServiceProvider serviceProvider, 
        IFileStorage fileStorage,
        IStringLocalizer<EnumDescriptionResources> stringLocalizer, 
        // IRabbitMqPublisherService publisherService,
        IWebAppPublisherService publisherService)
    {
        _serviceProvider = serviceProvider;
        _fileStorage = fileStorage;
        _stringLocalizer = stringLocalizer;
        _publisherService = publisherService;
        // _publisherService = publisherService;
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
            case WebApi.Domain.DataPredictionNnParameters dataPredictionNnParameters :
                model.PairFieldType = await ModelHelper.CompletionTheDictionaryAsync(
                    stream,
                    dataPredictionNnParameters.Separators);
                break;
            default:
                throw new Exception();
        };
        
        _publisherService.SendModelContract(model);

        // _publisherService.SendMessage(new ModelContract
        // {
        //     Id = model.Id,
        //     Name = model.Name,
        //     ModelType = model.ModelType,
        //     ModelStatus = model.ModelStatus,
        //     Parameters = fullParam,
        //     PairFieldType = model.PairFieldType
        // }, Queues.ToTrain);
        
        await dbContext.SaveChangesAsync();
    }

    public async Task Predict(Guid id, Dictionary<string, JsonElement> modelForPrediction)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        
        var model = dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new ArgumentException("The model with current id not found");
        }

        var fileName = await GetAsync(model.Id);
        
        _publisherService.SendPredictContract(model, modelForPrediction, fileName);
        
        // _publisherService.SendMessage(new PredictionContract
        // {
        //     Model = new ModelContract
        //     {
        //         Id = model.Id,
        //         Name = model.Name,
        //         ModelType = model.ModelType,
        //         ModelStatus = model.ModelStatus,
        //         Parameters = new (),
        //         PairFieldType = model.PairFieldType
        //     },
        //     ModelForPrediction = modelForPrediction,
        //     FileWithModelName = fileName
        // }, Queues.ToPredict);
    }
    
    private async Task<string?> GetAsync(Guid id)
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
        return fileWithModel.OriginalName;
    }
}
    