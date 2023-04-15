using NNTraining.Common.Enums;

namespace NNTraining.App;

public static class ModelHelper
{
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
}