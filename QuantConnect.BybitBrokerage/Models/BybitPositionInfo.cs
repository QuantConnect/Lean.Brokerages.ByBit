using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Position info
/// </summary>
public class BybitPositionInfo
{
    /// <summary>
    /// Product type
    /// </summary>
    public BybitAccountCategory Category { get; set; }
    
    /// <summary>
    /// Position idx, used to identify positions in different position modes
    /// </summary>
    [JsonProperty("positionIdx")]
    public PositionIndex? PositionIndex { get; set; }
    
    /// <summary>
    /// Symbol name
    /// </summary>
    public string Symbol { get; set; }
    
    /// <summary>
    /// Position side. Buy: long, Sell: short. Note: under one-way mode, it returns None if empty position
    /// </summary>
    public Enums.PositionSide Side { get; set; }
    /// <summary>
    /// Position size
    /// </summary>
    public decimal Size { get; set; }
    /// <summary>
    /// Average entry price
    /// For USDC Perp & Futures, it indicates average entry price, and it will not be changed with 8-hour session settlement
    /// </summary>
    [JsonProperty("avgPrice")] public decimal AveragePrice { get; set; }
    /// <summary>
    /// Position value
    /// </summary>
    public decimal PositionValue { get; set; }
    /// <summary>
    /// Unrealised PnL
    /// </summary>
    public decimal UnrealisedPnl { get; set; }
    /// <summary>
    /// Last mark price
    /// </summary>
    public decimal MarkPrice { get; set; }
}