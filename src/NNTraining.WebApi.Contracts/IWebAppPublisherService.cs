using System.Text.Json;
using NNTraining.Common.ServiceContracts;
using NNTraining.WebApi.Domain.Models;

namespace NNTraining.WebApi.Contracts;

public interface IWebAppPublisherService
{
    void SendModelContract(Model model);
    void SendPredictContract(Model model, Dictionary<string, JsonElement> modelForPrediction, string fileName);
}