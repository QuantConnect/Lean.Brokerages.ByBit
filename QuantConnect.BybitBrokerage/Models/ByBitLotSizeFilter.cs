/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using Newtonsoft.Json;

namespace QuantConnect.Brokerages.Bybit.Models;

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