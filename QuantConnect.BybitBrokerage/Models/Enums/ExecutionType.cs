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
/// Trade execution type
/// </summary>
public enum ExecutionType
{
    /// <summary>
    /// Trade
    /// </summary>
    Trade,
    /// <summary>
    /// Auto-Deleveraging <seealso href="https://www.bybit.com/en-US/help-center/bybitHC_Article?language=en_US&id=000001124"/>
    /// </summary>
    AdlTrade,
    /// <summary>
    /// Funding fee <seealso href="https://www.bybit.com/en-US/help-center/HelpCenterKnowledge/bybitHC_Article?id=000001123&language=en_US"/>
    /// </summary>
    Funding,
    /// <summary>
    /// Liquidation
    /// </summary>
    BustTrade,
    /// <summary>
    /// USDC Futures delivery
    /// </summary>
    Delivery,
    /// <summary>
    /// Block trade
    /// </summary>
    BlockTrade
}