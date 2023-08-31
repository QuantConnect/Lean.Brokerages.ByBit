using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Bybit product type
/// </summary>
public enum BybitProductCategory
{
    /// <summary>
    /// Spot
    /// </summary>
    [EnumMember(Value = "spot")] Spot,
    /// <summary>
    /// Linear futures
    /// </summary>
    [EnumMember(Value = "linear")] Linear,
    /// <summary>
    /// Inverse futures
    /// </summary>
    [EnumMember(Value = "inverse")] Inverse,
    /// <summary>
    /// Options
    /// </summary>
    [EnumMember(Value = "option")] Option
}

