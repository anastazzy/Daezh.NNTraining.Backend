using Microsoft.ML.Data;
using NNTraining.Common.Enums;

namespace NNTraining.App;

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
    public static async Task<Dictionary<string, Types>> CompletionTheDictionaryAsync(Stream fileStream, char[]? separators)
    {
        if (!fileStream.CanWrite || separators is null)
        {
            var parameter = separators is null ? nameof(separators) : nameof(fileStream);
            throw new ArgumentNullException(parameter);
        }
        
        var mapColumnNameColumnType = new Dictionary<string, Types>();
        fileStream.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(fileStream);
        
        //get headers
        var lineWithHeaders = await streamReader.ReadLineAsync();
        if (lineWithHeaders is null)
        {
            throw new ArgumentException("Headers is null");
        }
        var headers = lineWithHeaders.Split(separators);
    
        //get fields of first line
        var firstRow = await streamReader.ReadLineAsync();
        if (firstRow is null)
        {
            throw new ArgumentException("First row is null");
        }
        var fields = firstRow.Split(separators);
        
        //added values in dictionary with headers, values and type of this values
        for (var index = 0; index < fields.Length; index++)
        {
            var header = headers[index];
            var field = fields[index];
            
            var fieldsType = float.TryParse(field, out _)
                ? Types.Single
                : Types.String;
            try
            {
                mapColumnNameColumnType.TryAdd(header, fieldsType);
            }
            catch (Exception)
            {
                throw new ArgumentException("Key is null");
            }
        }
        return mapColumnNameColumnType;
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