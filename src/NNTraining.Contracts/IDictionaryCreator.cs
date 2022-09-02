namespace NNTraining.Contracts;

public interface IDictionaryCreator
{
    Task CompletionTheDictionaryAsync(string fileName, char[] separators);
    Type GetTypeOfCurrentFields(Dictionary<string, Type> dictionary);
    Dictionary<string, Type> GetDictionary();
}