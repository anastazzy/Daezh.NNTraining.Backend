using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.ML;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;

namespace NNTraining.Host;

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
        _mlContext.Model.Save(_trainedModel, _dataView, fileName);
        var idFile = await _storage.UploadAsync(model.Name!, ".zip", 
            new FileStream(fileName,FileMode.Open), 
            model.ModelType, 
            model.Id,
            FileType.SavedModelInStorage);
        return idFile;
    }

    public Task<ITrainedModel> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}