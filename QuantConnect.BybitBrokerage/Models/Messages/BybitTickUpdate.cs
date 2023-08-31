using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Messages;


/// <summary>
/// Todo rename
/// Tick update
/// </summary>
public class BybitTickUpdate
{
    /// <summary>
    /// Tick time
    /// </summary>
    [JsonConverter(typeof(BybitTimeConverter))]
    [JsonProperty("T")]
    public DateTime Time { get; set; }

    /// <summary>
    /// Symbol
    /// </summary>
    [JsonProperty("s")] public string Symbol { get; set; }
    
    /// <summary>
    /// Order side
    /// </summary>
    [JsonProperty("S")] public OrderSide Side { get; set; }

    /// <summary>
    /// Order value
    /// </summary>
    [JsonProperty("v")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal Value { get; set; }

    /// <summary>
    /// Order price
    /// </summary>
    [JsonProperty("p")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal Price { get; set; }

    /// <summary>
    /// Tick direction
    /// </summary>
    [JsonProperty("L")] public TickDirection TickType { get; set; }
    
    /// <summary>
    /// Order id
    /// </summary>
    [JsonProperty("i")] public string Id { get; set; }
    
    /// <summary>
    /// BT
    /// </summary>
    [JsonProperty("BT")] public bool Bt { get; set; }
}