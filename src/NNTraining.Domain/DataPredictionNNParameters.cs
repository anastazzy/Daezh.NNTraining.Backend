﻿namespace NNTraining.Domain;

public class DataPredictionNNParameters : NNParameters
{
    public string NameOfTargetColumn { get; set; }
    public bool HasHeader { get; set; }
    public char[] Separators { get; set; }
}