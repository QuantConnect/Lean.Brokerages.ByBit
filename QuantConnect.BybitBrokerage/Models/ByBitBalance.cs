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
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantConnect.Brokerages.Bybit.Models.Enums;

namespace QuantConnect.Brokerages.Bybit.Models;

/// <summary>
/// Balance info
/// </summary>
public class BybitBalance
{
    /// <summary>
    /// Account type
    /// </summary>
    public BybitAccountType AccountType { get; set; }

    /// <summary>
    /// Account LTV
    /// </summary>
    public decimal? AccountLtv { get; set; }

    /// <summary>
    /// Account initial margin rate
    /// </summary>
    [JsonProperty("accountIMRate")]
    public decimal? AccountInitialMarginRate { get; set; }

    /// <summary>
    /// Account maintenance margin rate
    /// </summary>
    [JsonProperty("accountMMRate")]
    public decimal? AccountMaintenanceMarginRate { get; set; }

    /// <summary>
    /// Account equity in USD
    /// </summary>
    public decimal? TotalEquity { get; set; }

    /// <summary>
    /// Total wallet balance in USD
    /// </summary>
    public decimal? TotalWalletBalance { get; set; }

    /// <summary>
    /// Total margin balance in USD
    /// </summary>
    public decimal? TotalMarginBalance { get; set; }

    /// <summary>
    /// Total available balance in USD
    /// </summary>
    public decimal? TotalAvailableBalance { get; set; }

    /// <summary>
    /// Unrealized profit and loss in USD
    /// </summary>
    [JsonProperty("totalPerpUPL")]
    public decimal? TotalPerpUnrealizedPnl { get; set; }

    /// <summary>
    /// Initial margin in USD
    /// </summary>
    public decimal? TotalInitialMargin { get; set; }

    /// <summary>
    /// Maintenance margin in USD
    /// </summary>
    public decimal? TotalMaintenanceMargin { get; set; }

    /// <summary>
    /// Asset info
    /// </summary>
    [JsonProperty("coin")]
    public IEnumerable<BybitAssetBalance> Assets { get; set; } = Array.Empty<BybitAssetBalance>();
}