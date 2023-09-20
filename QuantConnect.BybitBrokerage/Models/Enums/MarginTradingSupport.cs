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
/// Margin trading support for securities
/// </summary>
public enum MarginTradingSupport
{
    /// <summary>
    /// Regardless of normal account or UTA account, this trading pair does not support margin trading
    /// </summary>
    None,
    
    /// <summary>
    /// For both normal account and UTA account, this trading pair supports margin trading
    /// </summary>
    Both,
    
    /// <summary>
    /// Only for UTA account, this trading pair supports margin trading
    /// </summary>
    [EnumMember(Value = "utaOnly")]
    UTAOnly,
    
    /// <summary>
    /// Only for normal account, this trading pair supports margin trading
    /// </summary>
    NormalSpotOnly,
}