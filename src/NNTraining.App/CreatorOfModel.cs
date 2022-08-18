using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace NNTraining.Host;

public class CreatorOfModel
{
    private readonly MLContext _mlContext = new (0);
    private readonly string _nameOfTrainSet;
    private readonly Dictionary<string, Type> _dictionary = new ();

    public CreatorOfModel(string nameOfTrainSet)//train-set.csv
    {
        _nameOfTrainSet = nameOfTrainSet;
    }
    
    public async Task<float> Create()
    {
        var type = await GetTypeOfModelAndColumnsWithType(_nameOfTrainSet);

        //Do something with the choice of the desired method
        // const int countOfParametersOfNecessaryMethod = 3;
        // var infoLoader = typeof(TextLoaderSaverCatalog)
        //     .GetMethods()
        //     .FirstOrDefault(x => x.Name == nameof(TextLoaderSaverCatalog.LoadFromTextFile)
        //                          && x.IsGenericMethod
        //                          && x.GetParameters().Length == countOfParametersOfNecessaryMethod);
        //
        // // var infoLoader = typeof(TextLoaderSaverCatalog).GetMethod(
        // //     nameof(TextLoaderSaverCatalog.LoadFromTextFile), 
        // //     BindingFlags.Static | BindingFlags.Public, 
        // //     new []
        // //     {
        // //         typeof(DataOperationsCatalog),
        // //         typeof(string),
        // //         typeof(TextLoader.Options)
        // //     });
        //
        // if (infoLoader is null)
        // {
        //     throw new ArgumentException("The method was not created");
        // }
        // var methodInfo = infoLoader!.MakeGenericMethod(type);
        // var methodResult = methodInfo.Invoke(_mlContext.Model, new object[]
        // {
        //     Activator.CreateInstance(typeof(DataOperationsCatalog)), 
        //     _nameOfTrainSet, 
        //     true, 
        //     ';'
        // });
        // if (methodResult is null)
        // {
        //     throw new ArgumentException("Method not be created");
        // }
        //
        // var trainView = (IDataView) methodResult;
        //
        //
        
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

        var trainingView = _mlContext.Data.LoadFromTextFile(_nameOfTrainSet, new TextLoader.Options
        {
            HasHeader = true,
            Separators = new[]
            {
                ';',
            },
            Columns = columns.ToArray()

        });
        // ////////////// was
        // var trainingView = _mlContext.Data.LoadFromTextFile(_nameOfTrainSet, new TextLoader.Options
        // {
        //     HasHeader = true,
        //     Separators = new[]
        //     {
        //         ';'
        //     },
        //     Columns = new[]
        //     {
        //         new TextLoader.Column("square", DataKind.Single, 0),
        //         new TextLoader.Column("floor", DataKind.Single, 1),
        //         new TextLoader.Column("max_floor", DataKind.Single, 2),
        //         new TextLoader.Column("year", DataKind.Single, 3),
        //         new TextLoader.Column("is_combined_bathroom", DataKind.Single, 4),
        //         new TextLoader.Column("is_secondary_housing", DataKind.Single, 5),
        //         new TextLoader.Column("rooms_number", DataKind.Single, 6),
        //         new TextLoader.Column("renovation_type", DataKind.String, 7),
        //         new TextLoader.Column("price", DataKind.Single, 8),
        //     }
        // });
        // //

        var dataProcessPipeline = CreateTrainingPipeline("price", columns);
        // was
        // var dataProcessPipeline = _mlContext.Transforms.CopyColumns("label", "price")
        //     .Append(_mlContext.Transforms.NormalizeMeanVariance("square"))
        //     .Append(_mlContext.Transforms.NormalizeMeanVariance("floor"))
        //     .Append(_mlContext.Transforms.NormalizeMeanVariance("max_floor"))
        //     .Append(_mlContext.Transforms.NormalizeMeanVariance("year"))
        //     .Append(_mlContext.Transforms.NormalizeMeanVariance("rooms_number"))
        //     .Append(_mlContext.Transforms.NormalizeMeanVariance("is_combined_bathroom"))
        //     .Append(_mlContext.Transforms.NormalizeMeanVariance("is_secondary_housing"))
        //     .Append(_mlContext.Transforms.Categorical.OneHotEncoding("renovation_type_output", "renovation_type"))
        //     .Append(_mlContext.Transforms.Concatenate(
        //         "features",
        //         "square",
        //         "floor",
        //         "max_floor",
        //         "year",
        //         "rooms_number",
        //         "is_combined_bathroom",
        //         "is_secondary_housing",
        //         "renovation_type_output"));
        if (dataProcessPipeline is null)
        {
            throw new ArgumentException("Pipeline was not created");
        }
            
        var trainer = _mlContext.Regression.Trainers.Sdca("label", "features");
        var trainingPipeline = dataProcessPipeline.Append(trainer);
        
        var trainedModel = trainingPipeline.Fit(trainingView);

        // Define temp type
        
        // var fields = trainingView.Schema
        //     .Select(x => (x.Name, x.Type == NumberDataViewType.Single ? typeof(float) : typeof(string)));
        // var type = MyTypeBuilder.CompileResultType(fields);

        dynamic example = Activator.CreateInstance(type);
        example.square = 20;
        example.floor = 5;
        example.max_floor = 16;
        example.year = 2018;
        example.is_combined_bathroom = 1;
        example.is_secondary_housing = 1;
        example.rooms_number = 1;
        example.renovation_type = "renovation";    

        var method = typeof(ModelOperationsCatalog).GetMethod(nameof(ModelOperationsCatalog.CreatePredictionEngine),
            new []
        {
            typeof(ITransformer),
            typeof(bool),
            typeof(SchemaDefinition),
            typeof(SchemaDefinition),
        });
        var generic = method!.MakeGenericMethod(type, typeof(PredictionResult));
        dynamic predictor = generic.Invoke(_mlContext.Model, new object[]{ trainedModel, true, null, null });
        PredictionResult result = predictor.Predict(example);

        Console.WriteLine(result.Score);
        return result.Score;
    }
    
    
    private void UsingModel()
    {
        
    }
    
    
    

    private async Task<Type> GetTypeOfModelAndColumnsWithType(string fileName)
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

    private EstimatorChain<ColumnConcatenatingTransformer>? CreateTrainingPipeline(string nameOfTargetColumn,
        IEnumerable<TextLoader.Column> columns)
    {
        const string outputConcat = "features";
        var nameOfColumns = new List<string>();
        var newColumns = columns.Select(x=> x)
            .Where(x => x.Name != nameOfTargetColumn)
            .ToList();
        
        var temp = _mlContext.Transforms
            .CopyColumns("label", nameOfTargetColumn);

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

        var result = estimatorChain!
            .Append(_mlContext.Transforms.Concatenate(outputConcat, nameOfColumns.ToArray()));
        return result;
    }
}
//
// public class TypeValuePair
// {
//     public Type Type { get;}
//     public object Value { get;}
//     public TypeValuePair(Type type, object value)
//     {
//         Type = type;
//         Value = value;
//     }
// }