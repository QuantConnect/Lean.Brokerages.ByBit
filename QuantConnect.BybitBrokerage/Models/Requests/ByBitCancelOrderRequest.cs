using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Requests;

/// <summary>
/// Bybit cancel order api request
/// </summary>
public class ByBitCancelOrderRequest
{
    /// <summary>
    /// Product category
    /// </summary>
    public BybitProductCategory Category { get; set; }
    
    /// <summary>
    /// Symbol
    /// </summary>
    public string Symbol { get; set; }
    /// <summary>
    /// Order id
    /// </summary>
    public string OrderId { get; set; }
    
    /// <summary>
    /// Order link id
    /// </summary>
    public string OrderLinkId { get; set; }
    
    /// <summary>
    /// Order filter
    /// </summary>
    public OrderFilter? OrderFilter { get; set; }
}