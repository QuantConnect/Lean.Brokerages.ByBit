using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Position index, used to identify positions in different position modes
/// </summary>
public enum PositionIndex
{
    /// <summary>
    /// One way mode
    /// </summary>
    [EnumMember(Value = "0")]
    OneWayMode = 0,
    
    /// <summary>
    /// Two way mode - buy side
    /// </summary>
    [EnumMember(Value = "1")]
    BuySideTwoWay = 1,
    
    /// <summary>
    /// Two way mode - sell side
    /// </summary>
    [EnumMember(Value = "2")]
    SellSideTwoWay = 2
}