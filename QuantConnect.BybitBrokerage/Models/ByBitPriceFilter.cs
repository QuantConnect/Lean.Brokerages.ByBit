namespace QuantConnect.BybitBrokerage.Models;

public class ByBitPriceFilter
{
    public string MinPrice { get; set; }
    public string MaxPrice { get; set; }
    public string TickSize { get; set; }
}