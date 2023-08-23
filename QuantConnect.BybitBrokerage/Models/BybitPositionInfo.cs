using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

public class BybitPositionInfo
{
    public BybitAccountCategory Category { get; set; }
    [JsonProperty("positionIdx")]
    public int PositionIndex { get; set; }
    public string Symbol { get; set; }
    public Enums.PositionSide Side { get; set; }
    public decimal Size { get; set; }
    [JsonProperty("avgPrice")]
    public decimal AveragePrice { get; set; }
    public decimal PositionValue { get; set; }
    public decimal UnrealisedPnl { get; set; }
    public decimal MarkPrice { get; set; }
}