using System.Text.Json;
using System.Text.Json.Serialization;

namespace NNTraining.Domain.Tools;

public class CustomModelParametersConverter : JsonConverter<NNParameters>
{
    public override NNParameters? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, NNParameters value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}