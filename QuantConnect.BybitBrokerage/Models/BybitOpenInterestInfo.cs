using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Bybit open interest info
/// </summary>
public class BybitOpenInterestInfo
{
    /// <summary>
    /// The time of the information
    /// </summary>
    [JsonConverter(typeof(BybitTimeConverter))]
    [JsonProperty("timestamp")]
    public DateTime Time { get; set; }
    
    /// <summary>
    /// Open interest
    /// </summary>
    public decimal OpenInterest { get; set; }
}