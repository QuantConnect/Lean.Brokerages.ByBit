using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Margin mode
/// </summary>
public enum MarginMode
{
    /// <summary>
    /// Isolated margin
    /// </summary>
    [EnumMember(Value = "ISOLATED_MARGIN")]
    IsolatedMargin,
    
    /// <summary>
    /// Regular margin
    /// </summary>
    [EnumMember(Value = "REGULAR_MARGIN")] RegualarMargin,

    /// <summary>
    /// Portfolio margin
    /// </summary>
    [EnumMember(Value = "PORTFOLIO_MARGIN")]
    PortfolioMargin,
}