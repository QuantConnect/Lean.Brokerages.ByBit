using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage;

public class BybitWSTradeData
{
    [JsonConverter(typeof(BybitTimeConverter))]

    [JsonProperty("T")] public DateTime Time { get; set; }
    [JsonProperty("s")] public string Symbol { get; set; }
    [JsonProperty("S")] public OrderSide Side { get; set; }

    [JsonProperty("v")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal Value { get; set; }

    [JsonProperty("p")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]

    public decimal Price { get; set; }

    [JsonProperty("L")] public TickDirection TickType { get; set; }
    [JsonProperty("i")] public Guid Id { get; set; }
    [JsonProperty("BT")] public bool BT { get; set; }
}