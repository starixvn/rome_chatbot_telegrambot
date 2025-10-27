using System.Diagnostics.CodeAnalysis;
using Fanex.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TelegramBot.Importer.Helpers;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public abstract class JsonCreationConverter<T> : JsonConverter
{
    protected virtual T Create(Type type, JObject obj)
    {
        Type targetType = GetTargetType(type, obj);

        return (T)Activator.CreateInstance(targetType);
    }

    protected virtual Type GetTargetType(Type type, JObject obj)
    {
        return type;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            ValidateInputParams(reader, objectType, serializer);

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject obj = CreateJObject(reader);
            T target = Create(objectType, obj);
            JsonReader jsonReader = obj.CreateReader();
            jsonReader.FloatParseHandling = reader.FloatParseHandling;
            serializer.Populate(jsonReader, target);

            return target;
        }
        catch (Exception ex)
        {
            Logger.Log.Warn(ex.Message);
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    protected virtual JObject CreateJObject(JsonReader reader)
    {
        return JObject.Load(reader);
    }

    protected static void ValidateInputParams(JsonReader reader, Type type, JsonSerializer serializer)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader), "Reader cannot be null");
        }

        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "type cannot be null");
        }

        if (serializer == null)
        {
            throw new ArgumentNullException(nameof(serializer), "serializer cannot be null");
        }
    }
}