using NNTraining.Contracts;
using NNTraining.Domain;

namespace NNTraining.Host;

public class ModelInteractionService: IModelInteractionService
{
    public void Train(long id, NNParameters parameters)
    {
        throw new NotImplementedException();
    }

    public object Predict(object modelForPrediction)
    {
        throw new NotImplementedException();
    }
    // private TParametrs GetParameters<TDto>() where TDto: ModelInputDto<TParametrs>
    // {
    //     return T switch
    //     {
    //         ModelType.DataPrediction => new DataPredictionNNParameters(),
    //     };
    // }
    
    //
    // public Task CreateTheDataPrediction()
    // {
    //     return _creator.Create();
    // }
    //


    // public async Task<IModelCreator> Create()
    // {
    //     
    //     var factory = new ModelFactory(_dbContext);
    //     var creator = await factory.CreateModel(modelDto);
    // }

    // public Dictionary<string,string> GetSchemaOfModel()
    // {
    //     return _creator.GetSchemaOfModel().ToDictionary(x => x.Item1, x => x.Item2.ToString());
    // }
    //
    // public object UsingModel(Dictionary<string,string> inputModelForUsing)
    // {
    //     return _creator.UsingModel(inputModelForUsing);
    // }
}