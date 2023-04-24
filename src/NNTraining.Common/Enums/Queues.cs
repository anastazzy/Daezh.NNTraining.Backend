namespace NNTraining.Common.Enums;

public enum Queues
{
    /// <summary>
    /// Очередь на тренировку моделей
    /// </summary>
    ToTrain,
    
    /// <summary>
    /// Очередь на предсказание 
    /// </summary>
    ToPredict,
    
    /// <summary>
    /// Очередь на смену статуса модели 
    /// </summary>
    ChangeModelStatus,
    
    /// <summary>
    /// Очередь на запись результата предсказания модели 
    /// </summary>
    PredictionResult,
}