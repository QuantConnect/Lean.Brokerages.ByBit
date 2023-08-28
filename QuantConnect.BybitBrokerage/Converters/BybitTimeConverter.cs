using System;
using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Converters;

public class BybitTimeConverter : JsonConverter<DateTime>
{
    public override void WriteJson(JsonWriter writer, DateTime value, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

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

    public override bool CanRead => true;
    public override bool CanWrite => false;
}