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

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Bybit VIP Levels
/// todo would it make sense to move this to Lean and reuse it in the fee model?
/// </summary>
public enum BybitVIPLevel
{
    /// <summary>
    /// VIP 0
    /// </summary>
    VIP0,
    /// <summary>
    /// VIP 1
    /// </summary>
    VIP1,
    /// <summary>
    /// VIP 2
    /// </summary>
    VIP2,
    /// <summary>
    /// VIP 3
    /// </summary>
    VIP3,
    /// <summary>
    /// VIP 4
    /// </summary>
    VIP4,
    /// <summary>
    /// VIP 5
    /// </summary>
    VIP5,
    /// <summary>
    /// Supreme VIP
    /// </summary>
    SupremeVIP,
    /// <summary>
    /// Pro 1
    /// </summary>
    Pro1,
    /// <summary>
    /// Pro 2
    /// </summary>
    Pro2,
    /// <summary>
    /// Pro 3
    /// </summary>
    Pro3,
    /// <summary>
    /// Pro 4
    /// </summary>
    Pro4,
    /// <summary>
    /// Pro 5
    /// </summary>
    Pro5
}