using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Bybit account type
/// </summary>
public enum BybitAccountType
{
    /// <summary>
    /// Derivatives Account
    /// </summary>
    [EnumMember(Value = "CONTRACT")] Contract,

    /// <summary>
    /// Spot Account
    /// </summary>
    [EnumMember(Value = "SPOT")] Spot,

    /// <summary>
    /// ByFi Account (The service has been offline)
    /// </summary>
    [EnumMember(Value = "INVESTMENT")] Investment,

    /// <summary>
    /// USDC Account
    /// </summary>
    [EnumMember(Value = "OPTION")] Option,

    /// <summary>
    /// UMA or UTA
    /// </summary>
    [EnumMember(Value = "UNIFIED")] Unified,

    /// <summary>
    /// Funding Account
    /// </summary>
    [EnumMember(Value = "FUND")] Funding,
}