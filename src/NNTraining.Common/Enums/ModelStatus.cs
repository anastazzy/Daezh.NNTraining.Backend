namespace NNTraining.Common.Enums;

public enum ModelStatus
{
    /// <summary>
    /// Инициализована
    /// </summary>
    Initialized,
    
    /// <summary>
    /// Необходимо задать параметры
    /// </summary>
    NeedAParameters,
    
    /// <summary>
    /// Готова к тренировке
    /// </summary>
    ReadyToTraining,
    
    /// <summary>
    /// Ожидает тренировки
    /// </summary>
    WaitingTraining,
    
    /// <summary>
    /// В процессе тренировки
    /// </summary>
    StillTraining,
    
    /// <summary>
    /// Готова к использованию
    /// </summary>
    Trained,
    
    /// <summary>
    /// Используется
    /// </summary>
    StillPredict,
    
    /// <summary>
    /// Предсказание выполнено
    /// </summary>
    Done,
    
    /// <summary>
    /// Удалена
    /// </summary>
    Deleted, 
    
    /// <summary>
    /// Произошла ошибка тренировки
    /// </summary>
    ErrorOfTrainingModel,
    
    /// <summary>
    /// Произошла ошибка предсказания
    /// </summary>
    PredictionError
}