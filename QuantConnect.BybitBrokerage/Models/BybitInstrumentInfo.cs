using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage.Models;

public class BybitInstrumentInfo
{
    public string Symbol { get; set; }
    public string ContractType { get; set; }
    public string Status { get; set; }
    public string BaseCoin { get; set; }
    public string QuoteCoin { get; set; }
    public string SettleCoin { get; set; }
    //public string OptionsType { get; set; }
    public string LaunchTime { get; set; }
    public string DeliveryTime { get; set; }
    public string? DeliveryFeeRate { get; set; }
    public string PriceScale { get; set; }
    public ByBitPriceFilter PriceFilter { get; set; }
    public ByBitLotSizeFilter LotSizeFilter { get; set; }
    public ByBitLeverageFilter LeverageFilter { get; set; }
    [JsonConverter(typeof(ByBitBoolConverter))]
    public bool UnifiedMarginTrade { get; set; }
    public int FundingInterval { get; set; }
    
}