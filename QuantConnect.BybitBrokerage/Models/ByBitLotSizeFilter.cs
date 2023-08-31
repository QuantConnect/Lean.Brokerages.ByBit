using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Size attributes
/// </summary>
public class ByBitLotSizeFilter
{
    /// <summary>
    /// Maximum order quantity
    /// </summary>
    [JsonProperty("maxOrderQty")] public string MaxOrderQuantity { get; set; }
    
    /// <summary>
    /// Minimum order quantity
    /// </summary>
    [JsonProperty("minOrderQty")] public string MinOrderQuantity { get; set; }
    
    /// <summary>
    /// The step to increase/reduce order quantity
    /// </summary>
    [JsonProperty("qtyStep")] public decimal? QuantityStep { get; set; }

    /// <summary>
    /// Maximum order qty for PostOnly order
    /// </summary>
    [JsonProperty("postOnlyMaxOrderQty")] public string PostOnlyMaxOrderQuantity { get; set; }

    /// <summary>
    /// [Spot] The precision of base coin
    /// </summary>
    public decimal? BasePrecision { get; set; }
    
    /// <summary>
    /// [Spot] Minimum order amount
    /// </summary>
    [JsonProperty("minOrderAmt")]
    public decimal? MinOrderAmount { get; set; }
    
    /// <summary>
    /// [Spot] Minimum order amount
    /// </summary>
    [JsonProperty("maxOrderAmt")]
    public decimal? MaxOrderAmount { get; set; }
}