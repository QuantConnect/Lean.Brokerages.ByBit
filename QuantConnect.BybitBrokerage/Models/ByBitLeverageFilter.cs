namespace QuantConnect.BybitBrokerage.Models;

public class ByBitLeverageFilter
{
    public string MinLeverage { get; set; }
    public string MaxLeverage { get; set; }
    public string LeverageStep { get; set; }
}