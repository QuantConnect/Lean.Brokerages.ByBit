using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Bybit order book row
/// </summary>
[JsonConverter(typeof(BybitOrderBookRowJsonConverter))]
public class BybitOrderBookRow
{
    /// <summary>
    /// Bid or Ask price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Bid or Ask size. The delta data has size=0, which means that all quotations for this price have been filled or cancelled
    /// </summary>
    public decimal Size { get; set; }
}