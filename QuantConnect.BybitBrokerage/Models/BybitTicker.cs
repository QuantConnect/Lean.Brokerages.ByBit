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
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Ticker info
/// </summary>
public class BybitTicker
{
    /// <summary>
    /// Symbol name
    /// </summary>
    public string Symbol { get; set; }
    

    /// <summary>
    /// Percentage change of market price relative to 24h
    /// </summary>
    [JsonProperty("price24hPcnt")] public decimal? Price24HoursPercentage { get; set; }

    /// <summary>
    /// Last price
    /// </summary>
    public decimal? LastPrice { get; set; }

    /// <summary>
    /// Market price 24 hours ago
    /// </summary>
    [JsonProperty("prevPrice24h")] public decimal? PreviousPrice24Hours { get; set; }

    /// <summary>
    /// The highest price in the last 24 hours
    /// </summary>
    [JsonProperty("highPrice24h")] public decimal? HighPrice24Hours { get; set; }

    /// <summary>
    /// The lowest price in the last 24 hours
    /// </summary>
    [JsonProperty("lowPrice24h")] public decimal? LowPrice24Hours { get; set; }

    /// <summary>
    /// Market price an hour ago
    /// </summary>
    [JsonProperty("prevPrice1h")] public decimal? PreviousPrice1Hour { get; set; }

    /// <summary>
    /// Mark price
    /// </summary>
    public decimal? MarkPrice { get; set; }

    /// <summary>
    /// Index price
    /// </summary>
    public decimal? IndexPrice { get; set; }

    /// <summary>
    /// Open interest size
    /// </summary>
    public decimal? OpenInterest { get; set; }

    /// <summary>
    /// Open interest value
    /// </summary>
    public decimal? OpenInterestValue { get; set; }

    /// <summary>
    /// Turnover for 24h
    /// </summary>
    [JsonProperty("turnover24h")] public decimal? Turnover24Hours { get; set; }

    /// <summary>
    /// Volume for 24h
    /// </summary>
    [JsonProperty("volume24h")] public decimal? Volume24Hours { get; set; }

    /// <summary>
    /// Next funding time
    /// </summary>
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime? NextFundingTime { get; set; }

    /// <summary>
    /// Funding rate
    /// </summary>
    public decimal? FundingRate { get; set; }

    /// <summary>
    /// Best bid price
    /// </summary>
    public decimal? Bid1Price { get; set; }

    /// <summary>
    /// Best bid size
    /// </summary>
    public decimal? Bid1Size { get; set; }

    /// <summary>
    /// Best ask price
    /// </summary>
    public decimal? Ask1Price { get; set; }

    /// <summary>
    /// Best ask size
    /// </summary>
    public decimal? Ask1Size { get; set; }
}