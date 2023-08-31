using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Instrument info
/// </summary>
public class BybitInstrumentInfo
{
    /// <summary>
    /// Symbol name
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// Contract Type
    /// </summary>
    public string ContractType { get; set; }

    /// <summary>
    /// Instrument Status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Base coin
    /// </summary>
    public string BaseCoin { get; set; }

    /// <summary>
    /// Quote coin
    /// </summary>
    public string QuoteCoin { get; set; }

    /// <summary>
    /// Settle coin
    /// </summary>
    public string SettleCoin { get; set; }


    /// <summary>
    /// Launch time 
    /// </summary>
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime LaunchTime { get; set; }

    /// <summary>
    /// Delivery time
    /// </summary>
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime DeliveryTime { get; set; }

    /// <summary>
    /// Delivery fee rate
    /// </summary>
    public string DeliveryFeeRate { get; set; }

    /// <summary>
    /// Price scale 
    /// </summary>
    public string PriceScale { get; set; } //todo

    /// <summary>
    /// Price attributes
    /// </summary>
    public ByBitPriceFilter PriceFilter { get; set; }

    /// <summary>
    /// Size attributes
    /// </summary>
    public ByBitLotSizeFilter LotSizeFilter { get; set; }

    /// <summary>
    /// Leverage attributes
    /// </summary>
    public ByBitLeverageFilter LeverageFilter { get; set; }

    /// <summary>
    /// Whether to support unified margin trade
    /// </summary>
    [JsonConverter(typeof(ByBitBoolConverter))]
    public bool UnifiedMarginTrade { get; set; }

    /// <summary>
    /// Funding interval (minute)
    /// </summary>
    public int FundingInterval { get; set; }

    /// <summary>
    /// [Spot] Margin trade symbol or not
    /// - This is to identify if the symbol support margin trading under different account modes
    /// - You may find some symbols not supporting margin buy or margin sell, so you need to go to Collateral Info (UTA) or Borrowable Coin (Normal) to check if that coin is borrowable
    /// </summary>
    public MarginTradingSupport MarginTrading { get; set; }
}