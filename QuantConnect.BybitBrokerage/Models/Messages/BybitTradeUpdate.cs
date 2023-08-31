using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Messages;


/// <summary>
/// Trade update
/// </summary>
public class BybitTradeUpdate : BybitTrade
{
    /// <summary>
    /// Product type
    /// </summary>
    public BybitProductCategory Category { get; set; }
}