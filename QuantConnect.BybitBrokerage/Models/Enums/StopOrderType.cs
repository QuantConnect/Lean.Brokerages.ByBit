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

using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Stop order type
/// </summary>
public enum StopOrderType
{
    /// <summary>
    /// Unknown
    /// </summary>
    [EnumMember(Value = "UNKNOWN")] Unknown,
    /// <summary>
    /// Take profit
    /// </summary>
    TakeProfit,
    
    /// <summary>
    /// Stop-loss
    /// </summary>
    StopLoss,
    /// <summary>
    /// Trailing stop
    /// </summary>
    TrailingStop,
    
    /// <summary>
    /// Stop
    /// </summary>
    Stop,
    
    /// <summary>
    /// Partial take-profit
    /// </summary>
    PartialTakeProfit,
    
    /// <summary>
    /// Partial stop-loss
    /// </summary>
    PartialStopLoss,
    
    /// <summary>
    /// take-profit / stop-loss order
    /// </summary>
    [EnumMember(Value = "tpslOrder")] TpSlOrder,
}