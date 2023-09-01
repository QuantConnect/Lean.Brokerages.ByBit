using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Bybit order book update
/// </summary>
public class BybitOrderBookUpdate
{
    /// <summary>
    /// Symbol name
    /// </summary>
    [JsonProperty("s")]
    public string Symbol { get; set; }

    /// <summary>
    /// Asks. For snapshot stream, the element is sorted by price in ascending order
    /// </summary>
    [JsonProperty("a")]
    public BybitOrderBookRow[] Asks { get; set; }

    /// <summary>
    /// Bids. For snapshot stream, the element is sorted by price in descending order
    /// </summary>
    [JsonProperty("b")]
    public BybitOrderBookRow[] Bids { get; set; }

    /// <summary>
    /// Update ID. Is a sequence. Occasionally, you'll receive "u"=1, which is a snapshot data due to the restart of the service. So please overwrite your local orderbook
    /// </summary>
    [JsonProperty("u")]
    public long UpdateId { get; set; }

    /// <summary>
    /// Cross sequence. You can use this field to compare different levels orderbook data, and for the smaller seq, then it means the data is generated earlier
    /// </summary>
    [JsonProperty("seq")]
    public long CrossSequence { get; set; }
}