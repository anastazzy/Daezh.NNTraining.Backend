using System.Text.Json;
using System.Text.Json.Serialization;
using NNTraining.Common.ServiceContracts;

namespace NNTraining.Common.Options;

public class CustomModelParametersConverter : JsonConverter<NNParameters>
{
    public override NNParameters? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        if (!reader.Read()
            || reader.TokenType != JsonTokenType.PropertyName
            || reader.GetString() != "$type")
        {
            throw new JsonException();
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }
        
        var type = Type.GetType(reader.GetString());
        
        if (!reader.Read() || reader.GetString() != "$value")
        {
            throw new JsonException();
        }
        
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var result = (NNParameters?)JsonSerializer.Deserialize(ref reader, type);
        
        if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException();
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, NNParameters value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WriteString("$type", value.GetType().ToString());
        writer.WritePropertyName("$value");
        
        JsonSerializer.Serialize<object>(writer, value, options);

        writer.WriteEndObject();
    }
}