using System.Runtime.Serialization;
using NodaTime;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum DCPStatus
{
    [EnumMember(Value = "OFF")] Off,
    [EnumMember(Value = "ON")] On
}