using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Balance info
/// </summary>
public class BybitBalance
{
    /// <summary>
    /// Account type
    /// </summary>
    public BybitAccountType AccountType { get; set; }

    /// <summary>
    /// Account LTV
    /// </summary>
    public decimal? AccountLtv { get; set; }

    /// <summary>
    /// Account initial margin rate
    /// </summary>
    [JsonProperty("accountIMRate")]
    public decimal? AccountInitialMarginRate { get; set; }

    /// <summary>
    /// Account maintenance margin rate
    /// </summary>
    [JsonProperty("accountMMRate")]
    public decimal? AccountMaintenanceMarginRate { get; set; }

    /// <summary>
    /// Account equity in USD
    /// </summary>
    public decimal? TotalEquity { get; set; }

    /// <summary>
    /// Total wallet balance in USD
    /// </summary>
    public decimal? TotalWalletBalance { get; set; }

    /// <summary>
    /// Total margin balance in USD
    /// </summary>
    public decimal? TotalMarginBalance { get; set; }

    /// <summary>
    /// Total available balance in USD
    /// </summary>
    public decimal? TotalAvailableBalance { get; set; }

    /// <summary>
    /// Unrealized profit and loss in USD
    /// </summary>
    [JsonProperty("totalPerpUPL")]
    public decimal? TotalPerpUnrealizedPnl { get; set; }

    /// <summary>
    /// Initial margin in USD
    /// </summary>
    public decimal? TotalInitialMargin { get; set; }

    /// <summary>
    /// Maintenance margin in USD
    /// </summary>
    public decimal? TotalMaintenanceMargin { get; set; }

    /// <summary>
    /// Asset info
    /// </summary>
    [JsonProperty("coin")]
    public IEnumerable<BybitAssetBalance> Assets { get; set; } = Array.Empty<BybitAssetBalance>();
}