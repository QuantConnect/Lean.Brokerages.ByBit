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
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Account info
/// </summary>
public class BybitAccountInfo
{
    /// <summary>
    /// Account status
    /// </summary>
    public AccountUnifiedMarginStatus UnifiedMarginStatus { get; set; }
    
    /// <summary>
    /// Account margin mode
    /// </summary>
    public MarginMode MarginMode { get; set; }

    /// <summary>
    /// Disconnected-CancelAll-Prevention status: ON, OFF
    /// <seealso href="https://bybit-exchange.github.io/docs/v5/order/dcp"/>
    /// </summary>
    public DCPStatus DCPStatus { get; set; }

    /// <summary>
    /// DCP trigger time window which user pre-set. Between [3, 300] seconds, default: 10 sec
    /// </summary>
    public int TimeWindow { get; set; }

    /// <summary>
    /// SMP group ID. If the uid has no group, it's '0' by default.
    /// </summary>
    public int SmpGroup { get; set; }
    
    /// <summary>
    /// Whether the account is a master trade (copy trading)
    /// </summary>
    public bool IsMasterTrader { get; set; }

    /// <summary>
    /// Account data updated time
    /// </summary>
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime UpdateTime { get; set; }
}