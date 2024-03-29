﻿/*
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
using QuantConnect.Brokerages.Bybit.Models.Enums;

namespace QuantConnect.Brokerages.Bybit.Models;

/// <summary>
/// Position info
/// </summary>
public class BybitPositionInfo
{
    /// <summary>
    /// Product type
    /// </summary>
    public BybitProductCategory Category { get; set; }
    
    /// <summary>
    /// Position idx, used to identify positions in different position modes
    /// </summary>
    [JsonProperty("positionIdx")]
    public PositionIndex? PositionIndex { get; set; }
    
    /// <summary>
    /// Symbol name
    /// </summary>
    public string Symbol { get; set; }
    
    /// <summary>
    /// Position side. Buy: long, Sell: short. Note: under one-way mode, it returns None if empty position
    /// </summary>
    public Enums.PositionSide Side { get; set; }
    /// <summary>
    /// Position size
    /// </summary>
    public decimal Size { get; set; }
    /// <summary>
    /// Average entry price
    /// For USDC Perp & Futures, it indicates average entry price, and it will not be changed with 8-hour session settlement
    /// </summary>
    [JsonProperty("avgPrice")] public decimal AveragePrice { get; set; }
    /// <summary>
    /// Position value
    /// </summary>
    public decimal PositionValue { get; set; }
    /// <summary>
    /// Unrealised PnL
    /// </summary>
    public decimal UnrealisedPnl { get; set; }
    /// <summary>
    /// Last mark price
    /// </summary>
    public decimal MarkPrice { get; set; }
}