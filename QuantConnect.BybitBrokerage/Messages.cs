using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage;

public class BybitMessage
{
    public bool Success { get; set; }
    [JsonProperty("ret_msg")] public string ReturnMessage { get; set; }
    [JsonProperty("conn_id")] public Guid ConnectionId { get; set; }
    [JsonProperty("op")] public string Operation { get; set; }
}

public enum BybitWSMessageType
{
    [EnumMember]
    Snapshot,
}

public class BybitDataMessage<T>
{
    public string Topic { get; set; }
    public BybitWSMessageType Type { get; set; }

    [JsonProperty("ts")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime Time { get; set; }

    public T Data { get; set; }
}

public class BybitWSOrder : BybitOrder
{
    public BybitAccountCategory Category { get; set; }
}
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