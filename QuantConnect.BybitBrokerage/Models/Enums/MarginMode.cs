using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum MarginMode
{
    [EnumMember(Value = "ISOLATED_MARGIN")]
    IsolatedMargin,
    [EnumMember(Value = "REGULAR_MARGIN")] RegualarMargin,

    [EnumMember(Value = "PORTFOLIO_MARGIN")]
    PortfolioMargin,
}