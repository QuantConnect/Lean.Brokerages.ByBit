using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage;

public class BybitDataMessage<T>
{
    public string Topic { get; set; }
    public BybitWSMessageType Type { get; set; }

    [JsonProperty("ts")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime Time { get; set; }

    public T Data { get; set; }
}