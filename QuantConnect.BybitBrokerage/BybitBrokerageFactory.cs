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
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage
{
    /// <summary>
    /// Factory method to create Bybit brokerage
    /// </summary>
    public class BybitBrokerageFactory : BrokerageFactory
    {
        /// <summary>
        /// The name of the config key representing the requested order book depth
        /// <seealso href="https://bybit-exchange.github.io/docs/v5/websocket/public/orderbook"/>
        /// </summary>
        protected virtual string OrderBookDepthConfigName => "bybit-orderbook-depth";

        /// <summary>
        /// The default order book depth for this brokerage
        /// </summary>
        protected virtual int DefaultOrderBookDepth => 50;

        /// <summary>
        /// Gets the brokerage data required to run the brokerage from configuration/disk
        /// </summary>
        /// <remarks>
        /// The implementation of this property will create the brokerage data dictionary required for
        /// running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
        /// </remarks>
        public override Dictionary<string, string> BrokerageData => new()
        {
            { "bybit-api-secret", Config.Get("bybit-api-secret") },
            { "bybit-api-key", Config.Get("bybit-api-key") },
            // paper trading available using https://api-testnet.bybit.com
            { "bybit-api-url", Config.Get("bybit-api-url", "https://api.bybit.com") },
            // paper trading available using wss://stream-testnet.bybit.com
            { "bybit-websocket-url", Config.Get("bybit-websocket-url", "wss://stream.bybit.com") },
            { "bybit-vip-level", Config.Get("bybit-vip-level", "VIP0") },

            {
                OrderBookDepthConfigName,
                Config.Get(OrderBookDepthConfigName, DefaultOrderBookDepth.ToStringInvariant())
            },

            // load holdings if available
            { "live-holdings", Config.Get("live-holdings") }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="BybitBrokerageFactory"/> class
        /// </summary>
        public BybitBrokerageFactory() : base(typeof(BybitBrokerage))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BybitBrokerageFactory"/> class
        /// </summary>
        /// <param name="brokerageType">The brokerage type to be registered</param>
        protected BybitBrokerageFactory(Type brokerageType) : base(brokerageType)
        {
        }

        /// <summary>
        /// Gets a brokerage model that can be used to model this brokerage's unique behaviors
        /// </summary>
        /// <param name="orderProvider">The order provider</param>
        public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new BybitBrokerageModel();

        /// <summary>
        /// Creates a new IBrokerage instance
        /// </summary>
        /// <param name="job">The job packet to create the brokerage for</param>
        /// <param name="algorithm">The algorithm instance</param>
        /// <returns>A new brokerage instance</returns>
        public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
        {
            var errors = new List<string>();
            var apiKey = Read<string>(job.BrokerageData, "bybit-api-key", errors);
            var apiSecret = Read<string>(job.BrokerageData, "bybit-api-secret", errors);
            var apiUrl = Read<string>(job.BrokerageData, "bybit-api-url", errors);
            var wsUrl = Read<string>(job.BrokerageData, "bybit-websocket-url", errors);
            var vipLevel = Read<BybitVIPLevel>(job.BrokerageData, "bybit-vip-level", errors);
            var orderBookDepth = Read<int>(job.BrokerageData, OrderBookDepthConfigName, errors);

            if (errors.Count != 0)
            {
                // if we had errors then we can't create the instance
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }


            var agg = Composer.Instance.GetExportedValueByTypeName<IDataAggregator>(
                Config.Get("data-aggregator", "QuantConnect.Lean.Engine.DataFeeds.AggregationManager"),
                forceTypeNameOnExisting: false);

            var brokerage = CreateBrokerage(job, algorithm, agg, apiKey, apiSecret, apiUrl, wsUrl, vipLevel,
                orderBookDepth);
            Composer.Instance.AddPart(brokerage);
            return brokerage;
        }

        /// <summary>
        /// Creates a new IBrokerage instance
        /// </summary>
        /// <param name="job">The job packet to create the brokerage for</param>
        /// <param name="algorithm">The algorithm instance</param>
        /// <param name="aggregator">The aggregator for consolidating ticks</param>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        /// <param name="apiUrl">The rest api url</param>
        /// <param name="wsUrl">The websocket base url</param>
        /// <param name="orderBookDepth">The requested order book depth</param>
        /// <param name="vipLevel">Bybit VIP level</param>
        /// <returns>A new brokerage instance</returns>
        protected virtual IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm,
            IDataAggregator aggregator, string apiKey, string apiSecret, string apiUrl, string wsUrl,
            BybitVIPLevel vipLevel, int orderBookDepth)
        {
            return new BybitBrokerage(apiKey, apiSecret, apiUrl, wsUrl, algorithm, aggregator, job, orderBookDepth,
                vipLevel);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            //Not needed
        }
    }
}