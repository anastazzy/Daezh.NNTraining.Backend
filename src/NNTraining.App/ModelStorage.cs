using Microsoft.ML;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using NNTraining.Domain;

namespace NNTraining.App;

public class ModelStorage: IModelStorage// save model in minio?
{
    private readonly MLContext _mlContext;
    private readonly IFileStorage _storage;
    private readonly NNTrainingDbContext _dbContext;

    public ModelStorage(
        MLContext mlContext,
        NNTrainingDbContext dbContext,
        IFileStorage storage)
    {
        _mlContext = mlContext;
        _storage = storage;
        _dbContext = dbContext;
    }
    
    public Task<Guid> SaveAsync(ITrainedModel trainedModel, Model model, DataViewSchema dataViewSchema)
    {
        var contentType = "application/zip";
        var transformer = trainedModel as ITransformer;
        if (transformer is null)
        {
            return new Task<Guid>(() => Guid.Empty);
        }
        var idFile = Guid.NewGuid();// reflection in Model? - idFileInStorage
        var fileName = idFile + ".zip";
        _mlContext.Model.Save(transformer, dataViewSchema, fileName);
        using var stream =  new FileStream(fileName, FileMode.OpenOrCreate);
        return _storage.UploadAsync(
            fileName,
            contentType,
            stream,
            model.ModelType,
            model.Id,
            FileType.Model
        );
    }

    public async Task<ITrainedModel> GetAsync(Guid id, ModelType bucketName)
    {
        const string tempFileNameOfModel = "temp.zip";
        await _storage.GetAsync(id, bucketName, tempFileNameOfModel);
        
        var trainedModel = _mlContext.Model.Load(tempFileNameOfModel, out var modelSchema);
        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model?.PairFieldType is null)
        {
            throw new ArgumentException("The model or it`s field name type was not found");
        }
        var type = ModelHelper.GetTypeOfCurrentFields(model.PairFieldType);
        

        var trainedModelAsTransformer =
            trainedModel as TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>;
        if (trainedModelAsTransformer is null)
        {
            throw new ArgumentException("Error of conversion to transformer");
        }

        switch (bucketName)
        {
            case ModelType.DataPrediction:
            {
                var parameters = model.Parameters as DataPredictionNnParameters;
                if (parameters?.NameOfTargetColumn is null)
                {
                    throw new ArgumentException("Error of conversion parameters");
                }
                return new DataPredictionTrainedModel(trainedModelAsTransformer, _mlContext, type, parameters.NameOfTargetColumn);
            }
            default: throw new Exception();
        }
    }
}