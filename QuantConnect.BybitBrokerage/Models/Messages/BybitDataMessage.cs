using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage.Models.Messages;

public class BybitDataMessage<T>
{
    public string Topic { get; set; }
    public BybitMessageType Type { get; set; }

    [JsonProperty("ts")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime Time { get; set; }

    public T Data { get; set; }
}