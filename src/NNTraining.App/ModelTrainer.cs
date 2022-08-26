using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using NNTraining.Contracts;

namespace NNTraining.Host;

public class ModelTrainer
{
    private readonly MLContext _mlContext = new (0);
    private readonly string _nameOfTrainSet;
    private readonly Dictionary<string, Type> _dictionary;
    private TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>? _trainedModel;
    private Type? _type;
    private readonly string _nameOfTargetColumn;

    public ModelTrainer(string nameOfTrainSet, string nameOfTargetColumn)//"train-set.csv", "price"
    {
        _nameOfTargetColumn = nameOfTargetColumn;
        _nameOfTrainSet = nameOfTrainSet;
        _dictionary = new Dictionary<string, Type>();
    }
    
    
    
    public async Task CreateAndTrain()
    {
        _type = await GetTypeOfModelWithCompletionTheDictionaryAsync(_nameOfTrainSet);
        var columns = CreateTheTextLoaderColumn().ToArray();

        var trainingView = _mlContext.Data.LoadFromTextFile(_nameOfTrainSet, new TextLoader.Options
        {
            HasHeader = true,
            Separators = new[]
            {
                ';',
            },
            Columns = columns

        });

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
        _trainedModel = trainingPipeline.Fit(trainingView);
    }
    private IEnumerable<TextLoader.Column> CreateTheTextLoaderColumn()
    {
        List<TextLoader.Column> columns = new();
        var keys = _dictionary.Keys.ToArray();
        
        for (var index = 0; index < keys.Length; index++)
        {
            
            _dictionary.TryGetValue(keys[index], out var typeOfColumn);
            if (typeOfColumn is null)
            {
                throw new ArgumentException("Null value in dictionary.");
            }

            columns.Add(typeOfColumn == typeof(float)
                ? new TextLoader.Column(keys[index], DataKind.Single, index)
                : new TextLoader.Column(keys[index], DataKind.String, index));
        }
        return columns;
    }

    public IEnumerable<(string,Type)> GetSchemaOfModel()
    {
        if (_dictionary.Count == 0)
        {
            throw new ArgumentException("The Dictionary with columns does not have a information about columns");
        }
        foreach (var (key, value) in _dictionary)
        {
            yield return (key, value);
        }
    }


    private async Task<Type> GetTypeOfModelWithCompletionTheDictionaryAsync(string fileName)
    {
        using var streamReader = new StreamReader(fileName);
        
        //get headers
        var lineWithHeaders = await streamReader.ReadLineAsync();
        if (lineWithHeaders is null)
        {
            throw new ArgumentException("Headers is null");
        }
        var headers = lineWithHeaders.Split(';');

        //get fields of first line
        var firstRow = await streamReader.ReadLineAsync();
        if (firstRow is null)
        {
            throw new ArgumentException("First row is null");
        }
        var fields = firstRow.Split(';');
        
        //added values in dictionary with headers, values and type of this values
        for (var index = 0; index < fields.Length; index++)
        {
            var header = headers[index];
            var field = fields[index];
            
            var fieldsType = float.TryParse(field, out _)
                 ? typeof(float)
                 : typeof(string);
            try
            {
                _dictionary.TryAdd(header,fieldsType);
            }
            catch (Exception)
            {
                throw new ArgumentException("Key is null");
            }
        }
        
        var nameTypePair = _dictionary
            .Select(x => (x.Key, x.Value));
        
        return MyTypeBuilder.CompileResultType(nameTypePair);
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
        var result = estimatorChain!
            .Append(_mlContext.Transforms.Concatenate(outputConcat, nameOfColumns.ToArray()));
        return result;
    }
}
