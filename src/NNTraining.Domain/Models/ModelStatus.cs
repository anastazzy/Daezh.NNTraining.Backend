namespace NNTraining.Domain.Models;

public enum ModelStatus
{
    Initialized,
    NeedAParameters,
    ReadyToTraining,
    StillTraining,
    Trained,
    Deleted
}