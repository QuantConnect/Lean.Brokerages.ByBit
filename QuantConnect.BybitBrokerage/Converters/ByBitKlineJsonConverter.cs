using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.BybitBrokerage.Models;

namespace QuantConnect.BybitBrokerage.Converters;

public class ByBitKlineJsonConverter : JsonConverter<ByBitKLine>
{
    public override bool CanWrite => false;
    public override bool CanRead => true;

    public override void WriteJson(JsonWriter writer, ByBitKLine value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override ByBitKLine ReadJson(JsonReader reader, Type objectType, ByBitKLine existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type != JTokenType.Array || token.Count() != 7) throw new Exception();
        
        
        existingValue = existingValue ?? new ByBitKLine();
        existingValue.OpenTime = token[0].Value<long>();
        existingValue.Open = token[1].Value<decimal>();
        existingValue.High = token[2].Value<decimal>();
        existingValue.Low = token[3].Value<decimal>();
        existingValue.Close = token[4].Value<decimal>();
        existingValue.Turnover = token[5].Value<decimal>(); 
        existingValue.Volume = token[6].Value<decimal>();

        
        
        
        return existingValue;
    }
    
}