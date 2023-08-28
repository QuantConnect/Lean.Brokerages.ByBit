using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

public class BybitAccountInfo
{
    public UnifiedMarginStatus UnifiedMarginStatus { get; set; }
    public MarginMode MarginMode { get; set; }

    /// <summary>
    /// Disconnected-CancelAll-Prevention status: ON, OFF
    /// <seealso href="https://bybit-exchange.github.io/docs/v5/order/dcp"/>
    /// </summary>
    public DCPStatus DCPStatus { get; set; }

    /// <summary>
    /// DCP trigger time window which user pre-set. Between [3, 300] seconds, default: 10 sec
    /// </summary>
    public int TimeWindow { get; set; }

    public int SmpGroup { get; set; }
    public bool IsMasterTrade { get; set; }

    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime UpdateTime { get; set; }
}