namespace NNTraining.Domain.Enums;

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
    /// В процессе тренировки
    /// </summary>
    StillTraining,
    
    /// <summary>
    /// Готова к использованию
    /// </summary>
    Trained,
    
    /// <summary>
    /// Удалена
    /// </summary>
    Deleted, 
    
    /// <summary>
    /// Произошла ошибка тренировки
    /// </summary>
    ErrorOfTrainingModel
}