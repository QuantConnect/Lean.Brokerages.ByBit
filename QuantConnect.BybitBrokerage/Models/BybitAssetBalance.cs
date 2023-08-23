using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Asset balance info
/// </summary>
public class BybitAssetBalance
{
    /// <summary>
    /// Asset name
    /// </summary>
    [JsonProperty("coin")]
    public string Asset { get; set; } = string.Empty;
    /// <summary>
    /// Asset equity
    /// </summary>
    [JsonProperty("equity")]
    public decimal? Equity { get; set; }
    /// <summary>
    /// Asset usd value
    /// </summary>
    [JsonProperty("usdValue")]
    public decimal? UsdValue { get; set; }
    /// <summary>
    /// Asset balance
    /// </summary>
    [JsonProperty("walletBalance")]
    public decimal WalletBalance { get; set; }
    /// <summary>
    /// [Spot] Available balance
    /// </summary>
    [JsonProperty("free")]
    public decimal? Free { get; set; }
    /// <summary>
    /// [Spot] Locked balance
    /// </summary>
    [JsonProperty("locked")]
    public decimal? Locked { get; set; }
    /// <summary>
    /// Borrow amount
    /// </summary>
    [JsonProperty("borrowAmount")]
    public decimal? BorrowAmount { get; set; }
    /// <summary>
    /// Available borrow amount
    /// </summary>
    [JsonProperty("availableToBorrow")]
    public decimal? AvailableToBorrow { get; set; }
    /// <summary>
    /// Available withdrawal amount
    /// </summary>
    [JsonProperty("availableToWithdraw")]
    public decimal? AvailableToWithdraw { get; set; }
    /// <summary>
    /// Accrued interest
    /// </summary>
    [JsonProperty("accruedInterest")]
    public decimal? AccruedInterest { get; set; }
    /// <summary>
    /// Total order initial margin
    /// </summary>
    [JsonProperty("totalOrderIM")]
    public decimal? TotalOrderInitialMargin { get; set; }
    /// <summary>
    /// Total position maintenance marging
    /// </summary>
    [JsonProperty("totalPositionIM")]
    public decimal? TotalPositionInitialMargin { get; set; }
    /// <summary>
    /// Total position maintenance margin
    /// </summary>
    [JsonProperty("totalPositionMM")]
    public decimal? TotalPositionMaintenanceMargin { get; set; }
    /// <summary>
    /// Unrealized profit and loss
    /// </summary>
    [JsonProperty("unrealisedPnl")]
    public decimal? UnrealizedPnl { get; set; }
    /// <summary>
    /// Realized profit and loss
    /// </summary>
    [JsonProperty("cumRealisedPnl")]
    public decimal? RealizedPnl { get; set; }
    /// <summary>
    /// [Unified] Bonus
    /// </summary>
    public decimal? Bonus { get; set; }
}