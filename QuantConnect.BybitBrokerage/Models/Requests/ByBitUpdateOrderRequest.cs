namespace QuantConnect.BybitBrokerage.Models.Requests;

/// <summary>
/// Bybit update order api request
/// </summary>
public class ByBitUpdateOrderRequest : ByBitPlaceOrderRequest
{
    /// <summary>
    /// The order Id of the order which should be updated
    /// </summary>
    public string OrderId { get; set; }
}