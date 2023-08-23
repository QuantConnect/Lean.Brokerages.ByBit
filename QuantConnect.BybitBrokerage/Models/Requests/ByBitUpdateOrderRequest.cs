namespace QuantConnect.BybitBrokerage.Models.Requests;

public class ByBitUpdateOrderRequest : ByBitPlaceOrderRequest
{
    public string OrderId { get; set; }
}