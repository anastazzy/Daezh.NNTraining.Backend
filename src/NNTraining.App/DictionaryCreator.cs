using NNTraining.Contracts;

namespace NNTraining.App;

public class DictionaryCreator: IDictionaryCreator
{
    private readonly Dictionary<string, Type> _dictionary;

    public DictionaryCreator()
    {
        _dictionary = new Dictionary<string, Type>();
    }
    public async Task CompletionTheDictionaryAsync(string fileName, char[] separators)
    {
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
                _dictionary.TryAdd(header,fieldsType);
            }
            catch (Exception)
            {
                throw new ArgumentException("Key is null");
            }
        }
    }
    public Type GetTypeOfCurrentFields(Dictionary<string, Type> dictionary)
    {
        if (dictionary.Count < 1)
        {
            throw new ArgumentException("The dictionary is empty at the current moment");
        }
        var nameTypePair = dictionary
            .Select(x => (x.Key, x.Value));
        
        return MyTypeBuilder.CompileResultType(nameTypePair);
    }

    public Dictionary<string, Type> GetDictionary()
    {
        if (_dictionary.Count < 1)
        {
            throw new ArgumentException("The dictionary is empty at the current moment");
        }
        return _dictionary;
    }
}