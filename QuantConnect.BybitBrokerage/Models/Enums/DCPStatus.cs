using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Disconnect cancel all status
/// <seealso href="https://bybit-exchange.github.io/docs/v5/order/dcp"/>
/// </summary>
public enum DCPStatus
{
    /// <summary>
    /// Orders will stay open on disconnect
    /// </summary>
    [EnumMember(Value = "OFF")] Off,
    
    /// <summary>
    /// All open orders will be closed on disconnect
    /// </summary>
    [EnumMember(Value = "ON")] On
}