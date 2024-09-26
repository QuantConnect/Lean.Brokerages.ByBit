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

namespace QuantConnect.Brokerages.Bybit.Models.Enums;

/// <summary>
/// Bybit account unified margin status
/// </summary>
/// <see href="https://bybit-exchange.github.io/docs/v5/acct-mode#determine-account-mode-through-api"/>
public enum AccountUnifiedMarginStatus
{
    /// <summary>
    /// Contract transactions and spot transactions are separated
    /// </summary>
    [EnumMember(Value = "1")] ClassicAccount = 1,

    /// <summary>
    /// Inverse contract transactions are in a separate trading account,
    /// and the corresponding margin currency needs to be deposited into
    /// the "inverse derivatives account" before trading, and the margins are not shared between each other.
    /// For USDT perpetual, USDC perpetual, USDC Futures, spot and options are all traded within the "unified trading"
    /// </summary>
    [EnumMember(Value = "3")] UTA1 = 3,

    /// <summary>
    /// Inverse contract transactions are in a separate trading account,
    /// and the corresponding margin currency needs to be deposited into
    /// the "inverse derivatives account" before trading, and the margins are not shared between each other. 
    /// For USDT perpetual, USDC perpetual, USDC Futures, spot and options are all traded within the "unified trading"
    /// </summary>
    /// <remarks>
    /// Uta or uta (pro), they are the same thing, but pro has a slight performance advantage when trading via API
    /// </remarks>
    [EnumMember(Value = "4")] UTA1Pro = 4,

    /// <summary>
    /// The ultimate version of the unified account, integrating inverse contracts,
    /// USDT perpetual, USDC perpetual, USDC Futures, spot and options into a unified trading system.
    /// In cross margin and portfolio margin modes, margin is shared among all trades.
    /// </summary>
    [EnumMember(Value = "5")] UTA2 = 5,

    /// <summary>
    /// The ultimate version of the unified account, integrating inverse contracts,
    /// USDT perpetual, USDC perpetual, USDC Futures, spot and options into a unified trading system.
    /// In cross margin and portfolio margin modes, margin is shared among all trades.
    /// </summary>
    /// <remarks>
    /// Uta or uta (pro), they are the same thing, but pro has a slight performance advantage when trading via API
    /// </remarks>
    [EnumMember(Value = "6")] UTA2Pro = 6,
}