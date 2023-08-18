using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum PositionIndex
{
    [EnumMember(Value = "0")]

    OneWay = 0,
    [EnumMember(Value = "1")]

    Long =1, 
    [EnumMember(Value = "2")]

    Sell =2
}