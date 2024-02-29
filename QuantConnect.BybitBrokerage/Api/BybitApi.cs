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
using QuantConnect.Brokerages.Bybit.Models.Enums;
using QuantConnect.Securities;

namespace QuantConnect.Brokerages.Bybit.Api;

/// <summary>
/// Provides methods to work with the Bybit rest API
/// </summary>
public class BybitApi : IDisposable
{
    private static readonly Dictionary<BybitVIPLevel, int> RateLimits = new()
    {
        { BybitVIPLevel.VIP0, 10 },
        { BybitVIPLevel.VIP1, 20 },
        { BybitVIPLevel.VIP2, 40 },
        { BybitVIPLevel.VIP3, 60 },
        { BybitVIPLevel.VIP4, 60 },
        { BybitVIPLevel.VIP5, 60 },
        { BybitVIPLevel.SupremeVIP, 60 },
        { BybitVIPLevel.Pro1, 100 },
        { BybitVIPLevel.Pro2, 150 },
        { BybitVIPLevel.Pro3, 200 },
        { BybitVIPLevel.Pro4, 200 },
        { BybitVIPLevel.Pro5, 200 }
    };

    private readonly BybitApiClient _apiClient;
    private readonly string _apiKey;

    /// <summary>
    /// Market Api
    /// </summary>
    public BybitMarketApiEndpoint Market { get; }

    /// <summary>
    /// Account Api
    /// </summary>
    public BybitAccountApiEndpoint Account { get; }

    /// <summary>
    /// Position Api
    /// </summary>
    public BybitPositionApiEndpoint Position { get; }

    /// <summary>
    /// Trade Api
    /// </summary>
    public BybitTradeApiEndpoint Trade { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="BybitApi"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="restApiUrl">The api url</param>
    /// <param name="maxRequestsPerSecond">The request limit per second</param>
    public BybitApi(ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl,
        int maxRequestsPerSecond)
    {
        _apiKey = apiKey;
        _apiClient = new BybitApiClient(apiKey, apiSecret, restApiUrl, maxRequestsPerSecond);

        const string apiPrefix = "/v5";
        Market = new BybitMarketApiEndpoint(symbolMapper, apiPrefix, securityProvider, _apiClient);
        Account = new BybitAccountApiEndpoint(symbolMapper, apiPrefix, securityProvider, _apiClient);
        Position = new BybitPositionApiEndpoint(symbolMapper, apiPrefix, securityProvider, _apiClient);
        Trade = new BybitTradeApiEndpoint(Market, symbolMapper, apiPrefix, securityProvider, _apiClient);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BybitApi"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="restApiUrl">The api url</param>
    /// <param name="vipLevel">The accounts VIP level</param>
    public BybitApi(
        ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl,
        BybitVIPLevel vipLevel = BybitVIPLevel.VIP0) : this(symbolMapper, securityProvider, apiKey, apiSecret,
        restApiUrl, RateLimits.GetValueOrDefault(vipLevel, 10))
    {
    }

    /// <summary>
    /// Returns a websocket message which can be used to authenticate the private socket connection
    /// </summary>
    /// <param name="authenticationMessageValidFor">For how long the authentication token should be valid defaults to 10s</param>
    /// <returns></returns>
    public object AuthenticateWebSocket(TimeSpan? authenticationMessageValidFor = null)
    {
        var expires = DateTimeOffset.UtcNow.Add(authenticationMessageValidFor ?? TimeSpan.FromSeconds(10))
            .ToUnixTimeMilliseconds();
        var signString = $"GET/realtime{expires}";
        var signed = _apiClient.Sign(signString);
        return new
        {
            op = "auth",
            args = new object[]
            {
                _apiKey,
                expires,
                signed
            }
        };
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _apiClient.Dispose();
    }
}