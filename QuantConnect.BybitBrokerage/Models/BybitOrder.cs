using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Order info
/// </summary>
public class BybitOrder
{
    /// <summary>
    /// Order id
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Client order id
    /// </summary>
    [JsonProperty("orderLinkId")]
    public string? ClientOrderId { get; set; }

    /// <summary>
    /// Block trade id
    /// </summary>
    public string? BlockTradeId { get; set; }

    /// <summary>
    /// Symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Price
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    [JsonProperty("qty")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Order side
    /// </summary>
    public OrderSide Side { get; set; }

    /// <summary>
    /// Is leverage order
    /// </summary>
    [JsonConverter(typeof(ByBitBoolConverter))]
    public bool? IsLeverage { get; set; }

    /// <summary>
    /// Position mode
    /// </summary>
    [JsonProperty("positionIdx")]
    public PositionIndex? PositionMode { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    [JsonProperty("orderStatus")]
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Cancel type
    /// </summary>
    public CancelType? CancelType { get; set; }

    /// <summary>
    /// Reject reason
    /// </summary>
    public string? RejectReason { get; set; }

    /// <summary>
    /// Average fill pricec
    /// </summary>
    [JsonProperty("avgPrice")]
    public decimal? AveragePrice { get; set; }

    /// <summary>
    /// Quantity open
    /// </summary>
    [JsonProperty("leavesQty")]
    public decimal? QuantityRemaining { get; set; }

    /// <summary>
    /// Estimated value open
    /// </summary>
    [JsonProperty("leavesValue")]
    public decimal? ValueRemaining { get; set; }

    /// <summary>
    /// Quantity filled
    /// </summary>
    [JsonProperty("cumExecQty")]
    public decimal? QuantityFilled { get; set; }

    /// <summary>
    /// Value filled
    /// </summary>
    [JsonProperty("cumExecValue")]
    public decimal? ValueFilled { get; set; }

    /// <summary>
    /// Fee paid for filled quantity
    /// </summary>
    [JsonProperty("cumExecFee")]
    public decimal? ExecutedFee { get; set; }

    /// <summary>
    /// Time in force
    /// </summary>
    public TimeInForce TimeInForce { get; set; }

    /// <summary>
    /// Order type
    /// </summary>
    public OrderType OrderType { get; set; }

    /// <summary>
    /// Stop order type
    /// </summary>
    public StopOrderType? StopOrderType { get; set; }

    /// <summary>
    /// Order Iv
    /// </summary>
    public decimal? OrderIv { get; set; }

    /// <summary>
    /// Trigger price
    /// </summary>
    public decimal? TriggerPrice { get; set; }

    /// <summary>
    /// Take profit
    /// </summary>
    public decimal? TakeProfit { get; set; }

    /// <summary>
    /// Stop loss
    /// </summary>
    public decimal? StopLoss { get; set; }

    /// <summary>
    /// Take profit trigger type
    /// </summary>
    [JsonProperty("tpTriggerBy")]
    public TriggerType? TakeProfitTriggerBy { get; set; }

    /// <summary>
    /// Stop loss trigger type
    /// </summary>
    [JsonProperty("slTriggerBy")]
    public TriggerType? StopLossTriggerBy { get; set; }

    /// <summary>
    /// Trigger direction
    /// </summary>
    public TriggerDirection? TriggerDirection { get; set; }

    /// <summary>
    /// Trigger price type
    /// </summary>
    public TriggerType? TriggerBy { get; set; }

    /// <summary>
    /// Last price when the order was placed
    /// </summary>
    public decimal? LastPriceOnCreated { get; set; }

    /// <summary>
    /// Close on trigger
    /// </summary>
    public bool? CloseOnTrigger { get; set; }

    /// <summary>
    /// Reduce only
    /// </summary>
    public bool? ReduceOnly { get; set; }

    /// <summary>
    /// Create time
    /// </summary>
    [JsonProperty("createdTime")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// Update time
    /// </summary>
    [JsonProperty("updatedTime")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime UpdateTime { get; set; }
}