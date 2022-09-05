namespace NNTraining.Domain.Models;

public enum ModelStatus
{
    Created,
    ReadyToTraining,
    StillTraining,
    Trained,
    Deleted
}