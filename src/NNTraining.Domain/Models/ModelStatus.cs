namespace NNTraining.Domain.Models;

public enum ModelStatus
{
    Initialized,
    Created,
    ReadyToTraining,
    StillTraining,
    Trained,
    Deleted
}