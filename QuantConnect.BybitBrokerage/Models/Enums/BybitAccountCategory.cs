using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum BybitAccountCategory
{
    [EnumMember(Value = "spot")] Spot,
    [EnumMember(Value = "linear")] Linear,
    [EnumMember(Value = "inverse")] Inverse,
    [EnumMember(Value = "option")] Option
}