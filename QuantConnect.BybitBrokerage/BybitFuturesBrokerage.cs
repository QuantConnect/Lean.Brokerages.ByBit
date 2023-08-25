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
using QuantConnect.Securities.Crypto;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage
{

    [BrokerageFactory((typeof(BybitFuturesBrokerageFactory)))]
    public class BybitFuturesBrokerage : BybitBrokerage
    {
        
        
        public BybitFuturesBrokerage(string marketName = Market.Bybit) : base(marketName)
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
        public BybitFuturesBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,IOrderProvider orderProvider, ISecurityProvider securityProvider, IDataAggregator aggregator, LiveNodePacket job) : base(apiKey, apiSecret, restApiUrl,
            webSocketBaseUrl,orderProvider,securityProvider, aggregator, job)
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
        public BybitFuturesBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
            IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job) : base(apiKey, apiSecret, restApiUrl,
            webSocketBaseUrl, algorithm, aggregator, job)
        {
            
        }

        protected override BybitAccountCategory Category => BybitAccountCategory.Linear;

        protected override SecurityType GetSupportedSecurityType()
        {
            return SecurityType.CryptoFuture;
        }

        protected override bool CanSubscribe(Symbol symbol)
        {
            if (!base.CanSubscribe(symbol)) return false;
            
            //Can only subscribe to non-inverse pairs
            return CurrencyPairUtil.TryDecomposeCurrencyPair(symbol, out _, out var quoteCurrency) &&
                   quoteCurrency == "USDT";
        }
    }
}