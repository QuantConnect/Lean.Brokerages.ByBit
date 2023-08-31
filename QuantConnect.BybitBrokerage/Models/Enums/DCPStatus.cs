using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum DCPStatus
{
    [EnumMember(Value = "OFF")] Off,
    [EnumMember(Value = "ON")] On
}