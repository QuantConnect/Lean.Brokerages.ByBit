using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum PositionStatus
{
    
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