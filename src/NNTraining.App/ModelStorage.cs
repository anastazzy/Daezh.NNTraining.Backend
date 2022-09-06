﻿using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain;
using NNTraining.Domain.Models;
using File = System.IO.File;

namespace NNTraining.App;

public class ModelStorage: IModelStorage// save model in minio?
{
    private readonly MLContext _mlContext;
    private readonly NNTrainingDbContext _dbContext;
    private readonly IFileStorage _storage;

    public ModelStorage(
        MLContext mlContext,
        NNTrainingDbContext dbContext,
        IFileStorage storage)
    {
        _mlContext = mlContext;
        _dbContext = dbContext;
        _storage = storage;
    }
    public async Task<Guid> SaveAsync(ITrainedModel trainedModel, Model model, DataViewSchema dataView)
    {
        var fileName = model.Name + ".zip";
        var contentType = "application/zip";
        _mlContext.Model.Save(trainedModel.GetTransformer(), dataView, fileName);
        await using var stream = File.OpenRead(fileName);
        var idFile = await _storage.UploadAsync(model.Name!, contentType,
            stream, 
            model.ModelType, 
            model.Id,
            FileType.SavedModelInStorage);
        return idFile;
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