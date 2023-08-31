using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Order filter
/// </summary>
public enum OrderFilter
{
    /// <summary>
    /// Order
    /// </summary>
    Order,
    /// <summary>
    /// Stop order
    /// </summary>
    StopOrder,
    /// <summary>
    /// Take-profit Stop-loss order
    /// </summary>
    [EnumMember(Value = "tpslOrder")] TpSlOrder,
}