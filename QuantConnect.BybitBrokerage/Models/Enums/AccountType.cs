using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum AccountType
{
    /// <summary>
    /// Contract account (futures)
    /// </summary>
    [EnumMember(Value = "CONTRACT")] Contract,

    /// <summary>
    /// Spot account
    /// </summary>
    [EnumMember(Value = "SPOT")] Spot,

    /// <summary>
    /// Investment (defi) account
    /// </summary>
    [EnumMember(Value = "INVESTMENT")] Investment,

    /// <summary>
    /// Copy trading account
    /// </summary>
    [EnumMember(Value = "COPYTRADING")] CopyTrading,

    /// <summary>
    /// Option account
    /// </summary>
    [EnumMember(Value = "OPTION")] Option,

    /// <summary>
    /// Funding account
    /// </summary>
    [EnumMember(Value = "FUND")] Fund,

    /// <summary>
    /// Unified account
    /// </summary>
    [EnumMember(Value = "UNIFIED")] Unified,
}