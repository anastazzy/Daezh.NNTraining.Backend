using Microsoft.ML.Data;
using NNTraining.Common.Enums;

namespace NNTraining.TrainerWorker.App;

public static class ModelHelper
{
    public static Type GetTypeOfCurrentFields(Dictionary<string, Types> dictionary)
    {
        if (dictionary.Count < 1)
        {
            throw new ArgumentException("The dictionary is empty at the current moment");
        }
        var nameTypePair = dictionary
            .Select(x =>
            {
                var key = x.Key;
                var value = x.Value switch
                {
                    Types.Single => typeof(Single),
                    Types.String => typeof(string),
                    _ => throw new ArgumentException("Error in get current fields")
                };
                return (key, value);
            }).ToArray();
        
        return MyTypeBuilder.CompileResultType(nameTypePair);
    }
    
    public static IEnumerable<TextLoader.Column> CreateTheTextLoaderColumn(Dictionary<string, Types> dictionary)
    {
        List<TextLoader.Column> columns = new();
        var keys = dictionary.Keys.ToArray();
        
        for (var index = 0; index < keys.Length; index++)
        {
            dictionary.TryGetValue(keys[index], out var typeOfColumn);

            columns.Add(typeOfColumn == Types.Single
                ? new TextLoader.Column(keys[index], DataKind.Single, index)
                : new TextLoader.Column(keys[index], DataKind.String, index));
        }
        return columns;
    }
}