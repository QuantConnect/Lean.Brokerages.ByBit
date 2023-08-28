using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// User trade info
/// </summary>
public class BybitTrade
{
    /// <summary>
    /// Symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Order id trade belongs to
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Order Link Id
    /// </summary>
    public string? OrderLinkId { get; set; }

    /// <summary>
    /// Side
    /// </summary>
    public OrderSide Side { get; set; }

    /// <summary>
    /// Order price
    /// </summary>
    public decimal? OrderPrice { get; set; }

    /// <summary>
    /// Order quantity
    /// </summary>
    [JsonProperty("orderQty")]
    public decimal? OrderQuantity { get; set; }

    /// <summary>
    /// Remaining order quantity
    /// </summary>
    [JsonProperty("leavesQty")]
    public decimal? QuantityRemaining { get; set; }

    /// <summary>
    /// Order type
    /// </summary>
    public OrderType? OrderType { get; set; }

    /// <summary>
    /// Stop order type
    /// </summary>
    public StopOrderType? StopOrderType { get; set; }

    /// <summary>
    /// Fee paid
    /// </summary>
    [JsonProperty("execFee")]
    public decimal ExecutionFee { get; set; }

    /// <summary>
    /// Trade id
    /// </summary>
    [JsonProperty("execId")]
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// Trade price
    /// </summary>
    [JsonProperty("execPrice")]
    public decimal ExecutionPrice { get; set; }

    /// <summary>
    /// Trade quantity
    /// </summary>
    [JsonProperty("execQty")]
    public decimal ExecutionQuantity { get; set; }

    /// <summary>
    /// Trade value
    /// </summary>
    [JsonProperty("execValue")]
    public decimal? ExecutionValue { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    [JsonProperty("execTime")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime ExecutionTime { get; set; }

    /// <summary>
    /// Trade type
    /// </summary>
    [JsonProperty("execType")]
    public ExecutionType? ExecutionType { get; set; }

    /// <summary>
    /// Is maker
    /// </summary>
    public bool IsMaker { get; set; }

    /// <summary>
    /// Is leverage
    /// </summary>
    [JsonConverter(typeof(ByBitBoolConverter))]
    public bool? IsLeverage { get; set; }

    /// <summary>
    /// Fee rate
    /// </summary>
    public decimal? FeeRate { get; set; }

    /// <summary>
    /// [Options] Implied volatility
    /// </summary>
    public decimal? TradeIv { get; set; }

    /// <summary>
    /// [Options] Implied volatility of mark price
    /// </summary>
    public decimal? MarkIv { get; set; }

    /// <summary>
    /// Mark price
    /// </summary>
    public decimal? MarkPrice { get; set; }

    /// <summary>
    /// [Options] Index price
    /// </summary>
    public decimal? IndexPrice { get; set; }

    /// <summary>
    /// [Options] Underlying price
    /// </summary>
    public decimal? UnderlyingPrice { get; set; }

    /// <summary>
    /// Block trade id
    /// </summary>
    public string BlockTradeId { get; set; } = string.Empty;

    /// <summary>
    /// Closed position size
    /// </summary>
    [JsonProperty("closedSize")]
    public decimal? ClosedQuantity { get; set; }
}