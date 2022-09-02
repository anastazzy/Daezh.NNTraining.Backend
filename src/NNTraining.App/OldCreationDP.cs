using Microsoft.ML;
using Microsoft.ML.Data;

namespace NNTraining.App;

public class CreateANeuralModel
{
    public float Create()
    {
        var mlContext = new MLContext(0);

        var trainingView = mlContext.Data.LoadFromTextFile("train-set.csv", new TextLoader.Options
        {
            HasHeader = true,
            Separators = new []
            {
                ';'
            },
            Columns = new []
            {
                new TextLoader.Column("square", DataKind.Single, 0),
                new TextLoader.Column("floor", DataKind.Single, 1),
                new TextLoader.Column("max_floor", DataKind.Single, 2),
                new TextLoader.Column("year", DataKind.Single, 3),
                new TextLoader.Column("is_combined_bathroom", DataKind.Single, 4),
                new TextLoader.Column("is_secondary_housing", DataKind.Single, 5),
                new TextLoader.Column("rooms_number", DataKind.Single, 6),
                new TextLoader.Column("renovation_type", DataKind.String, 7),
                new TextLoader.Column("price", DataKind.Single, 8),
            }
        });

        var dataProcessPipeline = mlContext.Transforms.CopyColumns("label", "price")
            .Append(mlContext.Transforms.NormalizeMeanVariance("square"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("floor"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("max_floor"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("year"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("rooms_number"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("is_combined_bathroom"))
            .Append(mlContext.Transforms.NormalizeMeanVariance("is_secondary_housing"))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("renovation_type_output", "renovation_type"))
            .Append(mlContext.Transforms.Concatenate(
                "features",
                "square",
                "floor",
                "max_floor",
                "year",
                "rooms_number",
                "is_combined_bathroom",
                "is_secondary_housing",
                "renovation_type_output"));
            
        var trainer = mlContext.Regression.Trainers.Sdca("label", "features");
        var trainingPipeline = dataProcessPipeline.Append(trainer);

        var trainedModel = trainingPipeline.Fit(trainingView);

        // Define temp type
        var fields = trainingView.Schema
            .Select(x => (x.Name, x.Type == NumberDataViewType.Single ? typeof(float) : typeof(string)));
        var type = MyTypeBuilder.CompileResultType(fields);

        dynamic example = Activator.CreateInstance(type);
        example.square = 20;
        example.floor = 5;
        example.max_floor = 16;
        example.year = 2018;
        example.is_combined_bathroom = 1;
        example.is_secondary_housing = 1;
        example.rooms_number = 1;
        example.renovation_type = "renovation";    

        var method = typeof(ModelOperationsCatalog).GetMethod(nameof(ModelOperationsCatalog.CreatePredictionEngine), new []
        {
            typeof(ITransformer),
            typeof(bool),
            typeof(SchemaDefinition),
            typeof(SchemaDefinition),
        });
        var generic = method!.MakeGenericMethod(type, typeof(PredictionResult));
        dynamic predictor = generic.Invoke(mlContext.Model, new object[]{ trainedModel, true, null, null });
        PredictionResult result = predictor.Predict(example);

        Console.WriteLine(result.Score);
        return result.Score;
    }


}
class PredictionResult
{
    public float Score { get; init; }
}