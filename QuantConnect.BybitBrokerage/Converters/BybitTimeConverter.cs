using System;
using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Converters;

/// <summary>
/// Json converter to read Bybits time representation
/// </summary>
public class BybitTimeConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can read JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="JsonConverter"/> can read JSON; otherwise, <c>false</c>.</value>
    public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.</value>
    public override bool CanWrite => false;

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }


    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        long ts = 0;
        if (reader.Value is long lVal)
        {
            ts = lVal;
        }
        else if (reader.Value is string str)
        {
            if (!long.TryParse(str, out ts))
            {
                return existingValue;
            }
        }


        var dto = DateTimeOffset.FromUnixTimeMilliseconds(ts);
        return dto.DateTime;
    }
}