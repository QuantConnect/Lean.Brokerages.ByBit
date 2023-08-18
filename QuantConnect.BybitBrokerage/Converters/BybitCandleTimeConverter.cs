using System;
using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Converters;

public class BybitCandleTimeConverter : JsonConverter<DateTime>
{
    public override void WriteJson(JsonWriter writer, DateTime value, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
        Newtonsoft.Json.JsonSerializer serializer)
    {
        var str = (string?)reader.Value;
        if (str == null || !Decimal.TryParse(str, out var ts))
        {
            return existingValue;
        }

        return Convert(ts);
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;
    
    
    public static DateTime Convert(decimal ts)
    {
        var seconds = (long)ts;
        var sub = ts % 1;
        var rounded = Math.Round(sub, 3, MidpointRounding.ToZero) * 1000M;
        var ms = Math.Round(ts % 1M, 3) * 1000M;
        ms += seconds * 1000;
            
        var dto = DateTimeOffset.FromUnixTimeMilliseconds((long)ms);
        return dto.DateTime;
    }
}