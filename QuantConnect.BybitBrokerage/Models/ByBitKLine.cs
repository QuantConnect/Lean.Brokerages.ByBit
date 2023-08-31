namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// KLine info
/// </summary>
public class ByBitKLine
{
    /// <summary>
    /// Symbol name
    /// </summary>
    public string Symbol { get; set; }
    /// <summary>
    /// Kline interval 1,3,5,15,30,60,120,240,360,720,D,M,W
    /// </summary>
    public string Interval { get; set; }
    /// <summary>
    /// Start time of the candle
    /// </summary>
    public long OpenTime { get; set; }
    /// <summary>
    /// Open price
    /// </summary>
    public decimal Open { get; set; }
    /// <summary>
    /// Highest price
    /// </summary>
    public decimal High { get; set; }
    /// <summary>
    /// Lowest price
    /// </summary>
    public decimal Low { get; set; }
    /// <summary>
    /// Close price. It's the last traded price before the candle closed
    /// </summary>
    public decimal Close { get; set; }
    /// <summary>
    /// Trade volume. Unit of contract: pieces of contract. Unit of spot: quantity of coins
    /// </summary>
    public decimal Volume { get; set; }
    /// <summary>
    /// Turnover. Unit of figure: quantity of quota coin
    /// </summary>
    public decimal Turnover { get; set; }
}