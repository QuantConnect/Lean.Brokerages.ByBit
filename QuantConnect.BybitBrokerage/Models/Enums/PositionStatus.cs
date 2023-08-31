using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Position status
/// </summary>
public enum PositionStatus
{
    /// <summary>
    /// Normal
    /// </summary>
    Normal,
    /// <summary>
    /// In the liquidation process
    /// </summary>
    [EnumMember(Value = "Liq")]
    Liquidation,
    /// <summary>
    /// In the auto-deleverage process
    /// </summary>
    [EnumMember(Value = "Adl")]
    ADL,
}