using Microsoft.ML;
using NNTraining.Common;
using NNTraining.Common.Enums;
using NNTraining.Common.ServiceContracts;
using NNTraining.TrainerWorker.Contracts;

namespace NNTraining.TrainerWorker.App;

public class ModelStorage: IModelStorage
{
    private readonly MLContext _mlContext;
    private readonly ICustomMinioClient _storage;

    public ModelStorage(
        MLContext mlContext,
        ICustomMinioClient storage)
    {
        _mlContext = mlContext;
        _storage = storage;
    }
    
    public async Task<string> SaveAsync(ITrainedModel trainedModel, ModelContract model, DataViewSchema dataViewSchema)
    {
        var contentType = "application/zip";
        var transformer = trainedModel.GetTransformer();
        if (transformer is null)
        {
            throw new Exception();
        }
        
        var fileName = model.Id + ".zip";
        
        _mlContext.Model.Save(transformer, dataViewSchema, fileName);
        await using var stream =  new FileStream(fileName, FileMode.OpenOrCreate);
        var size = stream.Length;
        
        await _storage.UploadAsync(
            model.ModelType.ToString(),
            FileType.Model.ToString(),
            contentType,
            stream,
            size,
            fileName);
        
        return fileName;
    }

    public async Task<ITrainedModel> GetAsync(ModelContract model, string fileWithModelName, ModelType bucketName)
    {
        // var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        // if (model?.PairFieldType is null)
        // {
        //     throw new ArgumentException("The model or it`s field name type was not found");
        // }
        //
        // var modelFile = await _dbContext.ModelFiles.FirstOrDefaultAsync(x =>
        //     x.ModelId == id && x.FileType == FileType.Model);
        // if (modelFile is null)
        // {
        //     throw new ArgumentException("The file with this model was not found");
        // }
        //
        // var fileWithModel = await _dbContext.Files.FirstOrDefaultAsync(x => x.Id == modelFile.FileId);// при сохранении модели сделать нормальным тип файла
        // if (fileWithModel is null)
        // {
        //     throw new ArgumentException("The file with this model was not found");
        // }
        
        const string tempFileNameOfModel = "temp.zip";
        await _storage.GetObjectAsync(fileWithModelName, bucketName.ToString(), tempFileNameOfModel);
        
        var trainedModel = _mlContext.Model.Load(tempFileNameOfModel, out var modelSchema);
        
        var type = ModelHelper.GetTypeOfCurrentFields(model.PairFieldType);

        switch (bucketName)
        {
            case ModelType.DataPrediction:
            {
                var parameters = model.Parameters as DataPredictionNnParametersContract;
                if (parameters?.NameOfTargetColumn is null)
                {
                    throw new ArgumentException("Error of conversion parameters");
                }
                return new DataPredictionTrainedModel(trainedModel, _mlContext, type, parameters.NameOfTargetColumn);
            }
            default: throw new Exception();
        }
    }
}