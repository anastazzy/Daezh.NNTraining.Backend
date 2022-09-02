using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Models;

namespace NNTraining.App;

public class ModelStorage: IModelStorage// save model in minio?
{
    private readonly ITransformer _trainedModel;
    private readonly DataViewSchema _dataView;
    private readonly MLContext _mlContext;
    private readonly NNTrainingDbContext _dbContext;
    private readonly IFileStorage _storage;

    public ModelStorage(
        MLContext mlContext, 
        ITransformer trainedModel, 
        DataViewSchema dataView,
        NNTrainingDbContext dbContext,
        IFileStorage storage)
    {
        _trainedModel = trainedModel;
        _dataView = dataView;
        _mlContext = mlContext;
        _dbContext = dbContext;
        _storage = storage;
    }
    public async Task<Guid> SaveAsync(ITrainedModel trainedModel, Model model)
    {
        var fileName = model.Name + ".zip";
        _mlContext.Model.Save(trainedModel as ITransformer, _dataView, fileName);/////? было _trainedModel
        var idFile = await _storage.UploadAsync(model.Name!, ".zip", 
            new FileStream(fileName,FileMode.Open), 
            model.ModelType, 
            model.Id,
            FileType.SavedModelInStorage);
        return idFile;
    }

    public async Task<ITrainedModel> GetAsync(Guid id, ModelType bucketName)
    {
        var tempFileNameOfModel = "temp.zip";
        var a = await _storage.GetAsync(id, bucketName, tempFileNameOfModel);
        var trainedModel = _mlContext.Model.Load(tempFileNameOfModel, out var modelSchema);
        var dictionary = _dbContext.ModelFieldNameTypes.FirstOrDefault(x => x.IdModel == id);
        if (dictionary?.PairFieldType is null)
        {
            throw new ArgumentException("The dictionary was not found");
        }
        var dictionaryCreator = new DictionaryCreator();
        var type = dictionaryCreator.GetTypeOfCurrentFields(dictionary.PairFieldType);

        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model is null)
        {
            throw new  ArgumentException("The model was not found");
        }

        var targetColumn = "";

        switch (model.ModelType)
        {
            case ModelType.DataPrediction:
            {
                var parameters = model.Parameters as DataPredictionNnParameters;
                if (parameters is null)
                {
                    throw new ArgumentException("Error of conversion parameters");
                }
                targetColumn = parameters.NameOfTargetColumn;
                break;
            }
                
        }

        var trainedModelAsTransformer =
            trainedModel as TransformerChain<RegressionPredictionTransformer<LinearRegressionModelParameters>>;
        if (trainedModelAsTransformer is null)
        {
            throw new ArgumentException("Error of conversion to transformer");
        }
        return bucketName switch
        {
            ModelType.DataPrediction =>
                new DataPredictionTrainedModel(trainedModelAsTransformer, _mlContext, type, targetColumn!),
            _ => throw new Exception()
        };
        return trainedModel as ITrainedModel;
    }
}