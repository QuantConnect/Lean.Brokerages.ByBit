namespace QuantConnect.BybitBrokerage.Models;

public class ByBitKLine
{
    public string Symbol { get; set; }
    public string Interval { get; set; }
    public long OpenTime { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal Turnover { get; set; }
}