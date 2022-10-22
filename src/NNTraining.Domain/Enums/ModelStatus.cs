namespace NNTraining.Domain.Enums;

public enum ModelStatus
{
    Initialized,
    NeedAParameters,
    ReadyToTraining,
    StillTraining,
    Trained,
    Deleted
}