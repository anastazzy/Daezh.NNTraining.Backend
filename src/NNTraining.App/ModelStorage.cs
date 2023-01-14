using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using NNTraining.Domain;
using NNTraining.Domain.Enums;

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
    
    public async Task<string> SaveAsync(ITrainedModel trainedModel, Model model, DataViewSchema dataViewSchema)
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
        
        return await _storage.UploadAsync(
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
        var model = _dbContext.Models.FirstOrDefault(x => x.Id == id);
        if (model?.PairFieldType is null)
        {
            throw new ArgumentException("The model or it`s field name type was not found");
        }

        var modelFile = await _dbContext.ModelFiles.FirstOrDefaultAsync(x =>
            x.ModelId == id && x.FileType == FileType.Model);
        if (modelFile is null)
        {
            throw new ArgumentException("The file with this model was not found");
        }

        var fileWithModel = await _dbContext.Files.FirstOrDefaultAsync(x => x.Id == modelFile.FileId);// при сохранении модели сделать нормальным тип файла
        if (fileWithModel is null)
        {
            throw new ArgumentException("The file with this model was not found");
        }
        
        const string tempFileNameOfModel = "temp.zip";
        await _storage.GetAsync(fileWithModel.GuidName, bucketName, tempFileNameOfModel);
        
        var trainedModel = _mlContext.Model.Load(tempFileNameOfModel, out var modelSchema);
        
        var type = ModelHelper.GetTypeOfCurrentFields(model.PairFieldType);

        switch (bucketName)
        {
            case ModelType.DataPrediction:
            {
                var parameters = model.Parameters as DataPredictionNnParameters;
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