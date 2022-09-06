using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using NNTraining.Contracts;

namespace NNTraining.App;

public class DataPredictionTrainedModel: ITrainedModel
{
    private readonly TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>> _trainedModel;
    private readonly MLContext _mlContext;
    private readonly Type _type;
    private readonly string _nameOfTargetColumn;
    
    public DataPredictionTrainedModel(
        TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>> trainedModel,
        MLContext mlContext,
        Type type,
        string nameOfTargetColumn)
    {
        _trainedModel = trainedModel;
        _mlContext = mlContext;
        _type = type;
        _nameOfTargetColumn = nameOfTargetColumn;
    }
    public object Predict(object data)
    {
        Dictionary<string, string> inputModelForUsing;
        try
        {
            inputModelForUsing = (Dictionary<string, string>) data;
        }
        catch (InvalidCastException e)
        {
            throw new InvalidCastException("The data not be able passing in dictionary",e);
        }

        if (_type is null)
        {
            throw new ArgumentException("The type of model is null");
        }
        
        var instance = Activator.CreateInstance(_type);
        
        if (instance is null)
        {
            throw new ArgumentException("The instance of custom type was nat created");
        }
        var prop = instance.GetType()
            .GetProperties()
            .Where(x => x.Name != _nameOfTargetColumn);
        foreach (var currentPropertyInfo in prop)
        {
            var inputFieldValue = inputModelForUsing
                .Where(x => x.Key == currentPropertyInfo.Name)
                .Select(x => x.Value)
                .FirstOrDefault();
            if (currentPropertyInfo.PropertyType == typeof(string))
            {
                currentPropertyInfo.SetValue(instance, inputFieldValue);
            }
            else
            {
                if(float.TryParse(inputFieldValue, out var value))
                {
                    currentPropertyInfo.SetValue(instance, value);
                }
                else
                {
                    throw new ArgumentException("Error with parse of input value to Single");
                }
            }
        }
        
        var method = typeof(ModelOperationsCatalog).GetMethod(nameof(ModelOperationsCatalog.CreatePredictionEngine),
            new []
            {
                typeof(ITransformer),
                typeof(bool),
                typeof(SchemaDefinition),
                typeof(SchemaDefinition),
            });
        var generic = method!.MakeGenericMethod(_type, typeof(PredictionResult));
        if (_trainedModel is null)
        {
            throw new ArgumentException("The Model does not exist at the current moment");
        }
        dynamic predictor = generic.Invoke(_mlContext.Model, new object[]{ _trainedModel, true, null!, null! })!;
        if (predictor is null)
        {
            throw new ArgumentException("Error with invoke the generis function");
        }
        var a = (object) predictor;
        var engine = a.GetType().GetMethods()
            .Where(x => x.GetParameters().Length < 2)
            .FirstOrDefault(x => x.Name == "Predict");
        if (engine is null)
        {
            throw new ArgumentException("Engine is null");
        }
        var res = engine.Invoke((object) predictor, new []{instance});
        if (res is null)
        {
            throw new ArgumentException("Error with invoke the generis function");
        }
        var result = (PredictionResult) res;
        
        return result.Score;
    }

    public ITransformer GetTransformer()
    {
        return _trainedModel;
    }
}
class PredictionResult
{
    public float Score { get; init; }
}