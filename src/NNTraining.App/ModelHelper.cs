using Microsoft.ML.Data;

namespace NNTraining.App;

public static class ModelHelper
{
    public static Type GetTypeOfCurrentFields(Dictionary<string, Type> dictionary)
    {
        if (dictionary.Count < 1)
        {
            throw new ArgumentException("The dictionary is empty at the current moment");
        }
        var nameTypePair = dictionary
            .Select(x => (x.Key, x.Value));
        
        return MyTypeBuilder.CompileResultType(nameTypePair);
    }
    public static async Task<Dictionary<string, Type>> CompletionTheDictionaryAsync(string? fileName, char[]? separators)
    {
        if (string.IsNullOrEmpty(fileName) || separators is null)
        {
            var parameter = separators is null ? nameof(separators) : nameof(fileName);
            throw new ArgumentNullException(parameter);
        }
        var mapColumnNameColumnType = new Dictionary<string, Type>();
        using var streamReader = new StreamReader(fileName);
        
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
                ? typeof(float)
                : typeof(string);
            try
            {
                mapColumnNameColumnType.TryAdd(header,fieldsType);
            }
            catch (Exception)
            {
                throw new ArgumentException("Key is null");
            }
        }
        return mapColumnNameColumnType;
    }
    public static IEnumerable<TextLoader.Column> CreateTheTextLoaderColumn(Dictionary<string, Type> dictionary)
    {
        List<TextLoader.Column> columns = new();
        var keys = dictionary.Keys.ToArray();
        
        for (var index = 0; index < keys.Length; index++)
        {
            dictionary.TryGetValue(keys[index], out var typeOfColumn);
            if (typeOfColumn is null)
            {
                throw new ArgumentException("Null value in dictionary.");
            }

            columns.Add(typeOfColumn == typeof(float)
                ? new TextLoader.Column(keys[index], DataKind.Single, index)
                : new TextLoader.Column(keys[index], DataKind.String, index));
        }
        return columns;
    }
}