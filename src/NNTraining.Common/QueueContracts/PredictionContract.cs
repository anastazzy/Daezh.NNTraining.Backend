using System.Text.Json;
using NNTraining.Common.ServiceContracts;

namespace NNTraining.Common.QueueContracts;

public class PredictionContract
{
    public ModelContract? Model { get; set; }
    public Dictionary<string, JsonElement>? ModelForPrediction { get; set; }
    public string? FileWithModelName { get; set; }
}