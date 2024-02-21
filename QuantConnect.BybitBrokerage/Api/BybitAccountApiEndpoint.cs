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
/// Bybit account api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/account/wallet-balance"/>
/// </summary>
public class BybitAccountApiEndpoint : BybitApiEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitAccountApiEndpoint"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitAccountApiEndpoint(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider,
        BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
    {
    }

    /// <summary>
    /// Obtain wallet balance, query asset information of each currency, and account risk rate information
    /// </summary>
    /// <param name="accountType">The account type to fetch wallet balances for</param>
    /// <returns>The wallet balances</returns>
    public BybitBalance GetWalletBalances(BybitAccountType accountType)
    {
        if (accountType is not (BybitAccountType.Contract or BybitAccountType.Unified))
        {
            throw new ArgumentOutOfRangeException(nameof(accountType),
                "Wallet balances can only be fetched for 'UNIFIED' and 'CONTRACT'");
        }
        
        var parameters = new KeyValuePair<string, string>[]
        {
            new("accountType", accountType.ToStringInvariant().ToUpperInvariant())
        };

        var result =
            ExecuteGetRequest<BybitPageResult<BybitBalance>>("/account/wallet-balance", null, parameters, true);

        return result.List[0];
    }

    /// <summary>
    /// Query the margin mode configuration of the account.
    /// </summary>
    /// <returns>The account info</returns>
    public BybitAccountInfo GetAccountInfo()
    {
        return ExecuteGetRequest<BybitAccountInfo>("/account/info", authenticate: true);
    }
}