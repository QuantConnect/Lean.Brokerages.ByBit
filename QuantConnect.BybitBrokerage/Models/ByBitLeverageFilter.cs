namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Leverage attributes
/// </summary>
public class ByBitLeverageFilter
{
    /// <summary>
    /// Minimum leverage
    /// </summary>
    public string MinLeverage { get; set; }

    /// <summary>
    /// Maximum  leverage
    /// </summary>
    public string MaxLeverage { get; set; }

    /// <summary>
    /// The step to increase/reduce leverage
    /// </summary>
    public string LeverageStep { get; set; }
}