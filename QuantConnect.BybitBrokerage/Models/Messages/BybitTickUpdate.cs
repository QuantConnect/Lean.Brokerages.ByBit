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

using System;
using Newtonsoft.Json;
using QuantConnect.Brokerages.Bybit.Converters;
using QuantConnect.Brokerages.Bybit.Models.Enums;

namespace QuantConnect.Brokerages.Bybit.Models.Messages;


/// <summary>
/// Tick update
/// </summary>
public class BybitTickUpdate
{
    /// <summary>
    /// Tick time
    /// </summary>
    [JsonConverter(typeof(BybitTimeConverter))]
    [JsonProperty("T")]
    public DateTime Time { get; set; }

    /// <summary>
    /// Symbol
    /// </summary>
    [JsonProperty("s")] public string Symbol { get; set; }
    
    /// <summary>
    /// Order side
    /// </summary>
    [JsonProperty("S")] public OrderSide Side { get; set; }

    /// <summary>
    /// Order value (quantity)
    /// </summary>
    [JsonProperty("v")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Order price
    /// </summary>
    [JsonProperty("p")]
    [JsonConverter(typeof(BybitDecimalStringConverter))]
    public decimal Price { get; set; }

    /// <summary>
    /// Tick direction
    /// </summary>
    [JsonProperty("L")] public TickDirection TickType { get; set; }
    
    /// <summary>
    /// Order id
    /// </summary>
    [JsonProperty("i")] public string Id { get; set; }
    
    /// <summary>
    /// BT
    /// </summary>
    [JsonProperty("BT")] public bool Bt { get; set; }
}