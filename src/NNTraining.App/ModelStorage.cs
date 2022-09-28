using System.Threading.Tasks;
using Microsoft.ML;
using NNTraining.Contracts;
using NNTraining.DataAccess;
using NNTraining.Domain.Models;

namespace NNTraining.Host;

public class ModelStorage: IModelStorage// save model in minio?
{
    private readonly DataViewSchema _dataView;
    private readonly MLContext _mlContext;
    private readonly IFileStorage _storage;

    public ModelStorage(
        MLContext mlContext,  
        DataViewSchema dataView,
        IFileStorage storage)
    {
        _dataView = dataView;
        _mlContext = mlContext;
        _storage = storage;
    }
    public Task<Guid> SaveAsync(ITrainedModel trainedModel, Model model)
    {
        var contentType = "application/zip";
        var transformer = trainedModel as ITransformer;
        if (transformer is null)
        {
            return new Task<Guid>(() => Guid.Empty);
        }
        var idFile = Guid.NewGuid();// reflection in Model? - idFileInStorage
        var fileName = idFile + ".zip";
        _mlContext.Model.Save(transformer, _dataView, fileName);
        using var stream =  new FileStream(fileName, FileMode.OpenOrCreate);
        return _storage.UploadAsync(
            fileName,
            contentType,
            stream,
            stream.Length,
            model.ModelType.ToString(),
            model.Id,
            FileType.Model
        );
    }

    public Task<ITrainedModel> GetAsync(Guid id)
    {
        _storage.GetAsync(id,)
    }
}