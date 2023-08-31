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
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage
{
    /// <summary>
    /// Bybit futures brokerage implementation
    /// todo: inverse
    /// </summary>
    [BrokerageFactory((typeof(BybitFuturesBrokerageFactory)))]
    public class BybitFuturesBrokerage : BybitBrokerage
    {
        
        /// <summary>
        /// Account category
        /// </summary>
        protected override BybitProductCategory Category => BybitProductCategory.Linear;
        
        /// <summary>
        /// Parameterless constructor for brokerage
        /// </summary>
        public BybitFuturesBrokerage() : this(Market.Bybit)
        {
        }

        /// <summary>
        /// Constructor for brokerage
        /// </summary>
        public BybitFuturesBrokerage(string marketName) : base(marketName)
        {
        }
        
        /// <summary>
        /// Constructor for brokerage
        /// </summary>
        /// <param name="webSocketBaseUrl">The web socket base url</param>
        /// <param name="restApiUrl">The rest api url</param>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        /// <param name="algorithm">The algorithm instance is required to retrieve account type</param>
        /// <param name="orderProvider">The order provider is required to retrieve orders</param>
        /// <param name="securityProvider">The security provider is required</param>
        /// <param name="aggregator">The aggregator for consolidating ticks</param>
        /// <param name="job">The live job packet</param>
        /// <param name="vipLevel">Bybit VIP level</param>
        public BybitFuturesBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
            IAlgorithm algorithm, IOrderProvider orderProvider, ISecurityProvider securityProvider,
            IDataAggregator aggregator, LiveNodePacket job, BybitVIPLevel vipLevel = BybitVIPLevel.VIP0) : base(apiKey, apiSecret, restApiUrl,
            webSocketBaseUrl, algorithm, orderProvider, securityProvider, aggregator, job, Market.Bybit, vipLevel)
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
        public BybitFuturesBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
            IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job, BybitVIPLevel vipLevel = BybitVIPLevel.VIP0) : base(apiKey, apiSecret, restApiUrl,
            webSocketBaseUrl, algorithm, aggregator, job, vipLevel)
        {
        }



        /// <summary>
        /// Gets the supported security type by the brokerage
        /// </summary>
        protected override SecurityType GetSupportedSecurityType()
        {
            return SecurityType.CryptoFuture;
        }

        /// <summary>
        /// Checks if this brokerage supports the specified symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <returns>returns true if brokerage supports the specified symbol; otherwise false</returns>
        protected override bool CanSubscribe(Symbol symbol)
        {
            if (!base.CanSubscribe(symbol)) return false;

            //Can only subscribe to non-inverse pairs
            return CurrencyPairUtil.TryDecomposeCurrencyPair(symbol, out _, out var quoteCurrency) &&
                   quoteCurrency == "USDT";
        }
    }
}