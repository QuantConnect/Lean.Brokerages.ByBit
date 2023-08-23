using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Models;

public class ByBitLotSizeFilter
{
    [JsonProperty("maxOrderQty")] public string MaxOrderQuantity { get; set; }
    [JsonProperty("minOrderQty")] public string MinOrderQuantity { get; set; }
    [JsonProperty("qtyStep")] public decimal? QuantityStep { get; set; }
    

    [JsonProperty("postOnlyMaxOrderQty")]
    public string PostOnlyMaxOrderQuantity { get; set; }
    
    public decimal? BasePrecision { get; set; }
}