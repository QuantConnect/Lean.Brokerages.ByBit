using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Messages;

public class BybitTradeUpdate : BybitTrade
{
    public BybitAccountCategory Category { get; set; }
}