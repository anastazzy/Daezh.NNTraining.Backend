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

    private readonly IServiceProvider _serviceProvider;

    public ModelInteractionService(
        IServiceProvider serviceProvider, 
        IFileStorage fileStorage,
        IStringLocalizer<EnumDescriptionResources> stringLocalizer, 
        IWebAppPublisherService publisherService)
    {
        _serviceProvider = serviceProvider;
        _fileStorage = fileStorage;
        _stringLocalizer = stringLocalizer;
        _publisherService = publisherService;
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

        model.UpdateDate = DateTime.Now;
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

        var file = await dbContext.Files.FirstOrDefaultAsync(x =>
            x.ModelId == id && x.FileType == FileType.Model);
        if (file is null)
        {
            throw new ArgumentException("The file with this model was not found");
        }
        
        return file.OriginalName;
    }
}
    