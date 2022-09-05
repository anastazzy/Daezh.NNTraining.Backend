namespace NNTraining.App;

public static class Helper
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
}