using System.Text.Json;
using NNTraining.WebApi.Domain.Models;

namespace NNTraining.WebApi.Contracts;

public interface IWebAppPublisherService
{
    void SendModelContract(Model model);
    void SendPredictContract(Model model, Dictionary<string, JsonElement> modelForPrediction, string fileName);
}