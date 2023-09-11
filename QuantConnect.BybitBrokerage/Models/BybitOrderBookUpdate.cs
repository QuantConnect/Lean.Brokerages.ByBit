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

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Bybit order book update
/// </summary>
public class BybitOrderBookUpdate
{
    /// <summary>
    /// Symbol name
    /// </summary>
    [JsonProperty("s")]
    public string Symbol { get; set; }

    /// <summary>
    /// Asks. For snapshot stream, the element is sorted by price in ascending order
    /// </summary>
    [JsonProperty("a")]
    public BybitOrderBookRow[] Asks { get; set; }

    /// <summary>
    /// Bids. For snapshot stream, the element is sorted by price in descending order
    /// </summary>
    [JsonProperty("b")]
    public BybitOrderBookRow[] Bids { get; set; }

    /// <summary>
    /// Update ID. Is a sequence. Occasionally, you'll receive "u"=1, which is a snapshot data due to the restart of the service. So please overwrite your local orderbook
    /// </summary>
    [JsonProperty("u")]
    public long UpdateId { get; set; }

    /// <summary>
    /// Cross sequence. You can use this field to compare different levels orderbook data, and for the smaller seq, then it means the data is generated earlier
    /// </summary>
    [JsonProperty("seq")]
    public long CrossSequence { get; set; }
}