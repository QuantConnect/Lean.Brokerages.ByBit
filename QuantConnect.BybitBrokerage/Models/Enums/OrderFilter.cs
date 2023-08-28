using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum OrderFilter
{
    Order,
    StopOrder,
    [EnumMember(Value = "tpslOrder")] TpSlOrder,
}