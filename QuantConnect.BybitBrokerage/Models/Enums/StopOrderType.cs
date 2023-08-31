using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Stop order type
/// </summary>
public enum StopOrderType
{
    /// <summary>
    /// Unknown
    /// </summary>
    [EnumMember(Value = "UNKNOWN")] Unknown,
    /// <summary>
    /// Take profit
    /// </summary>
    TakeProfit,
    
    /// <summary>
    /// Stop-loss
    /// </summary>
    StopLoss,
    /// <summary>
    /// Trailing stop
    /// </summary>
    TrailingStop,
    
    /// <summary>
    /// Stop
    /// </summary>
    Stop,
    
    /// <summary>
    /// Partial take-profit
    /// </summary>
    PartialTakeProfit,
    
    /// <summary>
    /// Partial stop-loss
    /// </summary>
    PartialStopLoss,
    
    /// <summary>
    /// take-profit / stop-loss order
    /// </summary>
    [EnumMember(Value = "tpslOrder")] TpSlOrder,
}