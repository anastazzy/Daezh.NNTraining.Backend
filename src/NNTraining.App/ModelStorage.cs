using Microsoft.ML;
using NNTraining.Contracts;
using NNTraining.DataAccess;

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
    public Task<Guid> SaveAsync(ITrainedModel model)
    {
        // var idFile = Guid.NewGuid();// reflection in Model? - idFileInStorage
        // var fileName = idFile + ".zip";
        // _mlContext.Model.Save(_trainedModel, _dataView, fileName);
        // _storage.UploadAsync()
        throw new NotImplementedException();
    }

    public Task<ITrainedModel> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}