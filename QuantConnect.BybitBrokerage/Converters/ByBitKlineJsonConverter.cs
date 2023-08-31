using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.BybitBrokerage.Models;

namespace QuantConnect.BybitBrokerage.Converters;

/// <summary>
/// JSON converter to convert Bybit KLines
/// </summary>
public class ByBitKlineJsonConverter : JsonConverter<ByBitKLine>
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
    public override void WriteJson(JsonWriter writer, ByBitKLine value, JsonSerializer serializer)
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
    public override ByBitKLine ReadJson(JsonReader reader, Type objectType, ByBitKLine existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type != JTokenType.Array || token.Count() != 7) throw new Exception();


        existingValue = existingValue ?? new ByBitKLine();
        existingValue.OpenTime = token[0]!.Value<long>();
        existingValue.Open = token[1]!.Value<decimal>();
        existingValue.High = token[2]!.Value<decimal>();
        existingValue.Low = token[3]!.Value<decimal>();
        existingValue.Close = token[4]!.Value<decimal>();
        existingValue.Turnover = token[5]!.Value<decimal>();
        existingValue.Volume = token[6]!.Value<decimal>();


        return existingValue;
    }
}