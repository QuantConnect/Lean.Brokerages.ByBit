using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Requests;

public class ByBitCancelOrderRequest
{
    public BybitAccountCategory Category { get; set; }
    public string Symbol { get; set; }
    public string OrderId { get; set; }
    public string OrderLinkId { get; set; }
    public string OrderFilter { get; set; }
}