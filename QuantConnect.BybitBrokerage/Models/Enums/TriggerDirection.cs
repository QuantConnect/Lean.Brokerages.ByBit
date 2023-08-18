using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum TriggerDirection
{
    [EnumMember(Value = "1")]
    Rise =1,
    [EnumMember(Value = "2")]
    Fall =2
}