using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Messages;

public class BybitOrderUpdate : BybitOrder
{
    public BybitAccountCategory Category { get; set; }
}