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
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit position api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/position"/>
/// </summary>
public class BybitPositionApiEndpoint : BybitApiEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitPositionApiEndpoint"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitPositionApiEndpoint(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider,
        BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
    {
    }

    /// <summary>
    /// Query real-time position data, such as position size, cumulative realizedPNL.
    /// </summary>
    /// <param name="category">The product category</param>
    /// <returns>A list of all open positions in the current category</returns>
    public IEnumerable<BybitPositionInfo> GetPositions(BybitProductCategory category)
    {
        if (category == BybitProductCategory.Spot) return Array.Empty<BybitPositionInfo>();

        var parameters = new List<KeyValuePair<string, string>>();
        
        if (category == BybitProductCategory.Linear)
        {
            parameters.Add(KeyValuePair.Create("settleCoin", "USDT"));
        }

        return FetchAll<BybitPositionInfo>("/position/list", category, 200, parameters, true);
    }

    /// <summary>
    /// It supports to switch the position mode for USDT perpetual and Inverse futures.
    /// If you are in one-way Mode, you can only open one position on Buy or Sell side. If you are in hedge mode, you can open both Buy and Sell side positions simultaneously.
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="symbol">The symbol for which the mode should be changed</param>
    /// <param name="mode">The mode which should be set</param>
    public void SwitchPositionMode(BybitProductCategory category, Symbol symbol, PositionMode mode)
    {
        var ticker = SymbolMapper.GetBrokerageSymbol(symbol);
        var requestBody = new
        {
            category,
            mode = (int)mode,
            symbol = ticker
        };

        ExecutePostRequest<ByBitResponse>("/position/switch-mode", requestBody);
    }
}