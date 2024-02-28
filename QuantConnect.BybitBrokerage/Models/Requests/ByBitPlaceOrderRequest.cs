/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using Newtonsoft.Json;
using QuantConnect.Brokerages.Bybit.Converters;
using QuantConnect.Brokerages.Bybit.Models.Enums;

namespace QuantConnect.Brokerages.Bybit.Models.Requests;

/// <summary>
/// Bybit place order api request
/// </summary>
public class ByBitPlaceOrderRequest
{
    /// <summary>
    /// Trigger direction
    /// </summary>
    public int TriggerDirection { get; set; }

    /// <summary>
    /// Product category
    /// </summary>
    public BybitProductCategory Category { get; set; }

    /// <summary>
    /// Symbol
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// Whether to use margin. 0 (default): false (spot trading); 1: true (margin trading).
    /// Valid for spot trading in Unified Trading Account only.
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? IsLeverage { get; set; }

    /// <summary>
    /// Order side
    /// </summary>
    public OrderSide Side { get; set; }

    /// <summary>
    /// Order type
    /// </summary>
    public OrderType OrderType { get; set; }

    /// <summary>
    /// Valid for spot only. Order, tpslOrder, StopOrder. If not passed, Order by default
    /// </summary>
    public OrderFilter? OrderFilter { get; set; }

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

    /// <summary>
    /// Base price
    /// </summary>
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal? BasePrice { get; set; }

    /// <summary>
    /// Trigger price
    /// </summary>
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal? TriggerPrice { get; set; }

    /// <summary>
    /// Trigger by type
    /// </summary>
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
    public string OrderLinkId { get; set; }

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

    /// <summary>
    /// Reduce only order
    /// </summary>
    public bool? ReduceOnly { get; set; }

    /// <summary>
    /// Close on trigger
    /// </summary>
    public bool? CloseOnTrigger { get; set; }

    /// <summary>
    /// Market maker protection
    /// </summary>
    public bool? Mmp { get; set; }

    /// <summary>
    /// Position index
    /// </summary>
    [JsonProperty("positionIdx")] public int? PositionIndex { get; set; }

    /// <summary>
    /// Take-profit limit price
    /// </summary>
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal? TpLimitPrice { get; set; }

    /// <summary>
    /// Stop-loss limit price
    /// </summary>
    public decimal? SlLimitPrice { get; set; }

    /// <summary>
    /// Take-profit order type
    /// </summary>
    public OrderType? TpOrderType { get; set; }

    /// <summary>
    /// Stop-loss order type
    /// </summary>
    public OrderType? SlOrderType { get; set; }

    /// <summary>
    /// Take-profit / Stop-loss mode
    /// </summary>
    [JsonProperty("'tpslMode")] public string TpSlMode { get; set; }
}