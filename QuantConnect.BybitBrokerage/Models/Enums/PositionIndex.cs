using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Position index, used to identify positions in different position modes
/// </summary>
public enum PositionIndex
{
    [EnumMember(Value = "0")]
    OneWayMode = 0,
    [EnumMember(Value = "1")]
    BuySideTwoWay = 1,
    [EnumMember(Value = "2")]
    SellSideTwoWay = 2
}