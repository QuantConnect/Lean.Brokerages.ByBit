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
/// Bybit account unified margin status
/// </summary>
public enum AccountUnifiedMarginStatus
{
    /// <summary>
    /// Regular account
    /// </summary>
    [EnumMember(Value = "1")] Regular = 1,

    /// <summary>
    /// Unified margin account, it only trades linear perpetual and options.
    /// </summary>
    [EnumMember(Value = "2")] UnifiedMargin = 2,

    /// <summary>
    /// Unified trade account, it can trade linear perpetual, options and spot
    /// </summary>
    [EnumMember(Value = "3")] UnifiedTrade = 3,

    /// <summary>
    /// UTA Pro, the pro version of Unified trade account
    /// </summary>
    [EnumMember(Value = "4")] UTAPro = 4,
}