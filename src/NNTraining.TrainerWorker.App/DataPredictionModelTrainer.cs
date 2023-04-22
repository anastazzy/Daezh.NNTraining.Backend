using Microsoft.ML;
using Microsoft.ML.Data;
using NNTraining.Common.Enums;
using NNTraining.TrainerWorker.Contracts;

namespace NNTraining.TrainerWorker.App;

public class DataPredictionModelTrainer : IModelTrainer
{
    private readonly MLContext _mlContext = new (0);
    private readonly string _nameOfTrainSet;
    private readonly string _nameOfTargetColumn;
    private readonly bool _hasHeader;
    private readonly char[] _separators;

    public DataPredictionModelTrainer(string nameOfTrainSet, string nameOfTargetColumn,
        bool hasHeader, char[] separators) //"train-set.csv", "price", true, ';'
    {
        _nameOfTargetColumn = nameOfTargetColumn;
        _nameOfTrainSet = nameOfTrainSet;
        _hasHeader = hasHeader;
        _separators = separators;
    }

    private EstimatorChain<ColumnConcatenatingTransformer>? CreateTrainingPipeline(IEnumerable<TextLoader.Column> columns)
    {
        const string outputConcat = "features";
        var nameOfColumns = new List<string>();
        var newColumns = columns.Select(x=> x)
            .Where(x => x.Name != _nameOfTargetColumn)
            .ToList();
        
        var temp = _mlContext.Transforms
            .CopyColumns("label", _nameOfTargetColumn);

        EstimatorChain<ITransformer>? estimatorChain = null;
        
        foreach (var column in newColumns)
        {
            IEstimator<ITransformer> estimator;
            if (column.DataKind == DataKind.Single)
            {
                nameOfColumns.Add(column.Name);
                estimator = _mlContext.Transforms.NormalizeMeanVariance(column.Name);
            }
            else
            {
                var str = column.Name + "_output";
                nameOfColumns.Add(str);
                estimator = _mlContext.Transforms.Categorical.OneHotEncoding(str, column.Name);
            }

            estimatorChain = estimatorChain is null 
                ? temp.Append(estimator)
                : estimatorChain.Append(estimator);
        }

        if (estimatorChain is null)
        {
            throw new ArgumentException("EstimatorChain was not created");
        }
        var result = estimatorChain
            .Append(_mlContext.Transforms.Concatenate(outputConcat, nameOfColumns.ToArray()));
        return result;
    }

    public ITrainedModel Train(Dictionary<string, Types> mapColumnNameColumnType)
    {
        var columns = ModelHelper.CreateTheTextLoaderColumn(mapColumnNameColumnType).ToArray();

        var trainingView = _mlContext.Data.LoadFromTextFile(_nameOfTrainSet, new TextLoader.Options
        {
            HasHeader = _hasHeader,
            Separators = _separators,
            Columns = columns
        }); // почему-то не парсится (

        // creation the training pipelines
        var dataProcessPipeline = CreateTrainingPipeline(columns);
       
        if (dataProcessPipeline is null)
        {
            throw new ArgumentException("Pipeline was not created");
        }
        
        // create the trainer with specific algorithm  
        var trainer = _mlContext.Regression.Trainers.Sdca("label", "features");
        
        // train the model
        var trainingPipeline = dataProcessPipeline.Append(trainer);
        
        return new DataPredictionTrainedModel(
            trainingPipeline.Fit(trainingView), 
            _mlContext, 
            ModelHelper.GetTypeOfCurrentFields(mapColumnNameColumnType), 
            _nameOfTargetColumn);
    }
}
