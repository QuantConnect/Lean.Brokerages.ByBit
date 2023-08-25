using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

public class BybitTradeUpdate : BybitTrade
{
    public BybitAccountCategory Category { get; set; }
}