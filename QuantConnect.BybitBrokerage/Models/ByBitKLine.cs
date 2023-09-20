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

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// KLine info
/// </summary>
public class ByBitKLine
{
    /// <summary>
    /// Symbol name
    /// </summary>
    public string Symbol { get; set; }
    /// <summary>
    /// Kline interval 1,3,5,15,30,60,120,240,360,720,D,M,W
    /// </summary>
    public string Interval { get; set; }
    /// <summary>
    /// Start time of the candle
    /// </summary>
    public long OpenTime { get; set; }
    /// <summary>
    /// Open price
    /// </summary>
    public decimal Open { get; set; }
    /// <summary>
    /// Highest price
    /// </summary>
    public decimal High { get; set; }
    /// <summary>
    /// Lowest price
    /// </summary>
    public decimal Low { get; set; }
    /// <summary>
    /// Close price. It's the last traded price before the candle closed
    /// </summary>
    public decimal Close { get; set; }
    /// <summary>
    /// Trade volume. Unit of contract: pieces of contract. Unit of spot: quantity of coins
    /// </summary>
    public decimal Volume { get; set; }
    /// <summary>
    /// Turnover. Unit of figure: quantity of quota coin
    /// </summary>
    public decimal Turnover { get; set; }
}