using System.Text.Json;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.QueueContracts;
using NNTraining.Common.ServiceContracts;
using NNTraining.WebApi.Contracts;
using NNTraining.WebApi.Domain.Models;

namespace NNTraining.App.Utils;

public class WebAppPublisherService : IWebAppPublisherService
{
    private readonly IRabbitMqPublisherService _publisherService;

    public WebAppPublisherService(IRabbitMqPublisherService publisherService)
    {
        _publisherService = publisherService;
    }
    
    public void SendModelContract(Model model)
    {
        NNParameters fullParam;
        switch (model.Parameters)
        {
            case WebApi.Domain.DataPredictionNnParameters dataPredictionNnParameters :
                fullParam = new DataPredictionNnParameters
                {
                    NameOfTrainSet = dataPredictionNnParameters.NameOfTrainSet,
                    NameOfTargetColumn = dataPredictionNnParameters.NameOfTargetColumn,
                    HasHeader = dataPredictionNnParameters.HasHeader,
                    Separators = dataPredictionNnParameters.Separators
                };
                break;
            default:
                throw new Exception();
        };
        
        _publisherService.SendMessage(new ModelContract
        {
            Id = model.Id,
            Name = model.Name,
            ModelType = model.ModelType,
            ModelStatus = model.ModelStatus,
            Parameters = fullParam,
            PairFieldType = model.PairFieldType
        }, Queues.ToTrain);
    }

    public void SendPredictContract(Model model, Dictionary<string, JsonElement> modelForPrediction, string fileName)
    {
        NNParameters fullParam;
        switch (model.Parameters)
        {
            case WebApi.Domain.DataPredictionNnParameters dataPredictionNnParameters :
                fullParam = new DataPredictionNnParameters
                {
                    NameOfTrainSet = dataPredictionNnParameters.NameOfTrainSet,
                    NameOfTargetColumn = dataPredictionNnParameters.NameOfTargetColumn,
                    HasHeader = dataPredictionNnParameters.HasHeader,
                    Separators = dataPredictionNnParameters.Separators
                };
                break;
            default:
                throw new Exception();
        };
        
        _publisherService.SendMessage(new PredictionContract
        {
            Model = new ModelContract
            {
                Id = model.Id,
                Name = model.Name,
                ModelType = model.ModelType,
                ModelStatus = model.ModelStatus,
                Parameters = fullParam,
                PairFieldType = model.PairFieldType
            },
            ModelForPrediction = modelForPrediction,
            FileWithModelName = fileName
        }, Queues.ToPredict);
    }
}