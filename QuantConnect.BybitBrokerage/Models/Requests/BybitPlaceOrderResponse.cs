namespace QuantConnect.BybitBrokerage.Models.Requests;

/// <summary>
/// Bybit update order api response
/// </summary>
public class BybitUpdateOrderResponse
{
    /// <summary>
    /// Order Id
    /// </summary>
    public string OrderId { get; set; }
    
    /// <summary>
    /// Order link id
    /// </summary>
    public string OrderLinkId { get; set; }
}