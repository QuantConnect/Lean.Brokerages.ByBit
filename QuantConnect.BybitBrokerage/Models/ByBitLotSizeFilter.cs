using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

public class ByBitLotSizeFilter
{
    [JsonProperty("maxOrderQty")] public string MaxOrderQuantity { get; set; }
    [JsonProperty("minOrderQty")] public string MinOrderQuantity { get; set; }
    [JsonProperty("qtyStep")] public string QuantityStep { get; set; }
    

    [JsonProperty("postOnlyMaxOrderQty")]
    public string PostOnlyMaxOrderQuantity { get; set; }
}

public class BybitPositionInfo
{
    public BybitAccountCategory Category { get; set; }
    [JsonProperty("positionIdx")]
    public int PositionIndex { get; set; }
    public string Symbol { get; set; }
    public PositionSide Side { get; set; }
    public decimal Size { get; set; }
    [JsonProperty("avgPrice")]
    public decimal AveragePrice { get; set; }
    public decimal PositionValue { get; set; }
    public decimal UnrealisedPnl { get; set; }
    public decimal MarkPrice { get; set; }
}

public enum PositionSide
{
    None,
    Buy, Sell
}