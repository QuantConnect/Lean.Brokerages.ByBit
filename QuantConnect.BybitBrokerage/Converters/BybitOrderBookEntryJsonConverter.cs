using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.BybitBrokerage.Models;

namespace QuantConnect.BybitBrokerage.Converters;

/// <summary>
/// JSON converter to convert Bybit order book rows
/// </summary>
public class BybitOrderBookRowJsonConverter : JsonConverter<BybitOrderBookRow>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.</value>
    public override bool CanWrite => false;

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can read JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="JsonConverter"/> can read JSON; otherwise, <c>false</c>.</value>
    public override bool CanRead => true;

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, BybitOrderBookRow value, JsonSerializer serializer)
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
    public override BybitOrderBookRow ReadJson(JsonReader reader, Type objectType, BybitOrderBookRow existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type != JTokenType.Array || token.Count() != 2) throw new Exception();


        existingValue = existingValue ?? new BybitOrderBookRow();
        existingValue.Price = token[0]!.Value<decimal>();
        existingValue.Size = token[1]!.Value<decimal>();

        return existingValue;
    }
}