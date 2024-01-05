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

using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage;

/// <summary>
/// Bybit inverse futures brokerage implementation
/// </summary>
[BrokerageFactory(typeof(BybitInverseFuturesBrokerageFactory))]
public class BybitInverseFuturesBrokerage : BybitBrokerage
{
    protected override SecurityType[] SuppotedSecurityTypes { get; } = { SecurityType.Crypto, SecurityType.CryptoFuture };
    protected override BybitProductCategory[] SupportedBybitProductCategories { get; } = { BybitProductCategory.Inverse };
    protected override BybitAccountType WalletAccountType => BybitAccountType.Contract;

    /// <summary>
    /// Parameterless constructor for brokerage
    /// </summary>
    public BybitInverseFuturesBrokerage()
    {
    }
    
    /// <summary>
    /// Constructor for brokerage
    /// </summary>
    /// <param name="apiKey">api key</param>
    /// <param name="apiSecret">api secret</param>
    /// <param name="restApiUrl">The rest api url</param>
    /// <param name="webSocketBaseUrl">The web socket base url</param>
    /// <param name="algorithm">the algorithm instance is required to retrieve account type</param>
    /// <param name="aggregator">the aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    /// <param name="vipLevel">Bybit VIP level</param>
    public BybitInverseFuturesBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job,
        BybitVIPLevel vipLevel = BybitVIPLevel.VIP0)
        : base(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, algorithm, algorithm?.Portfolio?.Transactions,
            algorithm?.Portfolio, aggregator, job, vipLevel)
    {
    }

    /// <summary>
    /// Constructor for brokerage
    /// </summary>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="restApiUrl">The rest api url</param>
    /// <param name="webSocketBaseUrl">The web socket base url</param>
    /// <param name="algorithm">The algorithm instance is required to retrieve account type</param>
    /// <param name="orderProvider">The order provider is required to retrieve orders</param>
    /// <param name="securityProvider">The security provider is required</param>
    /// <param name="aggregator">The aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    /// <param name="vipLevel">Bybit VIP level</param>
    public BybitInverseFuturesBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IOrderProvider orderProvider, ISecurityProvider securityProvider,
        IDataAggregator aggregator, LiveNodePacket job, BybitVIPLevel vipLevel = BybitVIPLevel.VIP0)
        : base(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, algorithm, orderProvider,
            securityProvider, aggregator, job, vipLevel)
    {
    }
}