using System;
using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Converters;

/// <summary>
/// Json converter to support Bybits various bool return types
/// </summary>
public class ByBitBoolConverter : JsonConverter
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.</value>
    public override bool CanWrite => false;

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
        if (Nullable.GetUnderlyingType(objectType) != null)
        {
            return Nullable.GetUnderlyingType(objectType) == typeof(bool);
        }

        return objectType == typeof(bool);
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
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
}