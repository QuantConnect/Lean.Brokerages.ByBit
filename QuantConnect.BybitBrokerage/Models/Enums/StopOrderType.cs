using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum StopOrderType
{
    [EnumMember(Value = "UNKNOWN")]
    Unknown,
    TakeProfit,
    StopLoss,
    TrailingStop,
    Stop,
    PartialTakeProfit,
    PartialStopLoss,
    [EnumMember(Value = "tpslOrder")]
    TPSLOrder,
    


}