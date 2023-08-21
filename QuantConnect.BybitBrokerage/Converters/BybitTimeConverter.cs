using System;
using System.Globalization;
using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Converters;


public class BybitDecimalStringConverter : JsonConverter<decimal>
{
    public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
    {
      writer.WriteValue(value.ToStringInvariant());
    }

    public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var val = reader.Value;
        if (val is decimal dec)
        {
            return dec;
        }
        if (val is string str && decimal.TryParse(str, NumberStyles.Currency,CultureInfo.InvariantCulture, out var res))
        {
            return res;
        }

        return ReadJson(reader, objectType, existingValue, hasExistingValue, serializer);
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}

public class BybitTimeConverter : JsonConverter<DateTime>
{
    public override void WriteJson(JsonWriter writer, DateTime value, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
        Newtonsoft.Json.JsonSerializer serializer)
    {
        try
        {

            long ts = 0;
            if (reader.Value is long lVal)
            {
                ts = lVal;
            }
            else if (reader.Value is string str)
            {
                if (str == null || !long.TryParse(str, out ts))
                {
                    return existingValue;
                }
            }


            var dto = DateTimeOffset.FromUnixTimeMilliseconds(ts);
            return dto.DateTime;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;

}