using System;
using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Converters;

public class ByBitBoolConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        if (Nullable.GetUnderlyingType(objectType) != null)
        {
            return Nullable.GetUnderlyingType(objectType) == typeof(bool);
        }

        return objectType == typeof(bool);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        switch (reader.Value?.ToString()?.ToLower().Trim())
        {
            case "true":
            case "yes":
            case "y":
            case "1":
                return true;
            case "false":
            case "no":
            case "n":
            case "0":
                return false;
        }


        return new JsonSerializer().Deserialize(reader, objectType);
    }

    public override bool CanWrite => false;
}