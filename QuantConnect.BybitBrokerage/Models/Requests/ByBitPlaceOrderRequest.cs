using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Requests;

public class ByBitPlaceOrderRequest
{
    public int TriggerDirection { get; set; }
    public BybitAccountCategory Category { get; set; }
    public string Symbol { get; set; }

    /// <summary>
    /// Order side
    /// </summary>
    public OrderSide Side { get; set; }

    /// <summary>
    /// Order type
    /// </summary>
    public OrderType OrderType { get; set; }

    /// <summary>
    /// Order price
    /// </summary>
    [JsonConverter(typeof(BybitDecimalStringConverter))]

    public decimal? Price { get; set; }

    /// <summary>
    /// Order quantity
    /// </summary>
    [JsonProperty("qty")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]

    public decimal Quantity { get; set; }

    /// <summary>
    /// Time in force
    /// </summary>
    [JsonProperty("time_in_force")]
    public TimeInForce? TimeInForce { get; set; }

    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal? BasePrice { get; set; }
    [JsonConverter(typeof(BybitDecimalStringConverter))]

    public decimal? TriggerPrice { get; set; }
    public TriggerType? TriggerBy { get; set; }


    /// <summary>
    /// Implied volatility, for options only; parameters are passed according to the real value; for example, for 10%, 0.1 is passed.
    /// </summary>
    [JsonProperty("orderlv")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]

    public decimal? OrderLv { get; set; }

    /// <summary>
    /// User customized order ID. A max of 36 characters. A user cannot reuse an orderLinkId, with some exceptions. Combinations of numbers, letters (upper and lower cases), dashes, and underscores are supported. Not required for futures, but required for options.
    /// 1. The same orderLinkId can be used for both USDC PERP and USDT PERP.
    /// 2. An orderLinkId can be reused once the original order is either Filled or Cancelled
    /// </summary>
    public string? OrderLinkId { get; set; }

    /// <summary>
    /// Take-profit price, only valid when positions are opened.
    /// </summary>
    [JsonConverter(typeof(BybitDecimalStringConverter))]

    public decimal? TakeProfit { get; set; }

    /// <summary>
    /// Stop-loss price, only valid when positions are opened.

    /// </summary>
    ///     [JsonConverter(typeof(BybitDecimalStringConverter))]
    [JsonConverter(typeof(BybitDecimalStringConverter))]

    public decimal? StopLoss { get; set; }

    /// <summary>
    /// Type of take-profit activation price, LastPrice by default.
    /// </summary>
    public TriggerType? TpTriggerBy { get; set; }

    /// <summary>
    /// Type of stop-loss activation price, LastPrice by default.
    /// </summary>
    public TriggerType? SlTriggerBy { get; set; }

    public bool? ReduceOnly { get; set; }
    public bool? CloseOnTrigger { get; set; }
    public bool? Mmp { get; set; }
    
    [JsonProperty("positionIdx")]
    public int? PositionIndex
    {
        get;
        set;
    }


}