using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Margin trading support for securities
/// </summary>
public enum MarginTradingSupport
{
    /// <summary>
    /// Regardless of normal account or UTA account, this trading pair does not support margin trading
    /// </summary>
    None,
    
    /// <summary>
    /// For both normal account and UTA account, this trading pair supports margin trading
    /// </summary>
    Both,
    
    /// <summary>
    /// Only for UTA account, this trading pair supports margin trading
    /// </summary>
    [EnumMember(Value = "utaOnly")]
    UTAOnly,
    
    /// <summary>
    /// Only for normal account, this trading pair supports margin trading
    /// </summary>
    NormalSpotOnly,
}