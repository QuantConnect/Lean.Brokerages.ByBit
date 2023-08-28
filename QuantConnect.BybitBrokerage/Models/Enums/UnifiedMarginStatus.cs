using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum UnifiedMarginStatus
{
    /// <summary>
    /// Regular account
    /// </summary>
    [EnumMember(Value = "1")] Regular,

    /// <summary>
    /// Unified margin account, it only trades linear perpetual and options.
    /// </summary>
    [EnumMember(Value = "2")] UnifiedMargin,

    /// <summary>
    /// Unified trade account, it can trade linear perpetual, options and spot
    /// </summary>
    [EnumMember(Value = "3")] UnifiedTrade,

    /// <summary>
    /// UTA Pro, the pro version of Unified trade account
    /// </summary>
    [EnumMember(Value = "4")] UTAPro,
}