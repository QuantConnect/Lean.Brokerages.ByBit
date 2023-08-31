namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Bybit instrument price attributes
/// </summary>
public class ByBitPriceFilter
{
    /// <summary>
    /// Minimum order price
    /// </summary>
    public string MinPrice { get; set; }
    /// <summary>
    /// Maximum order price
    /// </summary>
    public string MaxPrice { get; set; }
    /// <summary>
    /// The step to increase/reduce order price
    /// </summary>
    public string TickSize { get; set; }
}