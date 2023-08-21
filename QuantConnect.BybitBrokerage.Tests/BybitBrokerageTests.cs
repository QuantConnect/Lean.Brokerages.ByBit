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
using Moq;
using NUnit.Framework;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.Tests;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Tests.Brokerages;
using QuantConnect.Tests.Common.Securities;

namespace QuantConnect.BybitBrokerage.Tests
{
    [TestFixture]
    public partial class BybitBrokerageTests : BrokerageTests
    {
        protected static Symbol BTCUSDT = Symbol.Create("BTCUSDT", SecurityType.CryptoFuture, "bybit");
        private BybitRestApiClient _client;
        protected override Symbol Symbol { get; } = BTCUSDT;
        protected override SecurityType SecurityType { get; }

        protected virtual ISymbolMapper SymbolMapper => new SymbolPropertiesDatabaseSymbolMapper(Market.Bybit);

        protected override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
        {
            var securities = new SecurityManager(new TimeKeeper(DateTime.UtcNow, TimeZones.Utc))
            {
                { Symbol, CreateSecurity(Symbol) }
            };

            var transactions = new SecurityTransactionManager(null, securities);
            transactions.SetOrderProcessor(new FakeOrderProcessor());

          /*  var algorithm = new Mock<IAlgorithm>();
            algorithm.Setup(a => a.Transactions).Returns(transactions);
            algorithm.Setup(a => a.BrokerageModel).Returns(new BybitFuturesBrokerageModel());
            algorithm.Setup(a => a.Portfolio)
                .Returns(new SecurityPortfolioManager(securities, transactions, new AlgorithmSettings()));
*/
            var apiKey = Config.Get("bybit-api-key");
            var apiSecret = Config.Get("bybit-api-secret");
            var apiUrl = Config.Get("bybit-api-url", "https://api-testnet.bybit.com");
            var websocketUrl = Config.Get("bybit-websocket-url", "wss://stream-testnet.bybit.com");

            _client = new BybitRestApiClient(SymbolMapper, null, apiKey, apiSecret, apiUrl);

            return new BybitFuturesBrokerage(apiKey, apiSecret, apiUrl, websocketUrl,
                new AggregationManager(), null, orderProvider);
        }

        protected override bool IsAsync() => false;

        protected override decimal GetAskPrice(Symbol symbol)
        {            var brokerageSymbol = SymbolMapper.GetBrokerageSymbol(symbol);
            return _client.GetTicker(BybitAccountCategory.Linear, brokerageSymbol).Ask1Price;

        }


        /// <summary>
        /// Provides the data required to test each order type in various cases
        /// </summary>
        private static TestCaseData[] OrderParameters()
        {
            return new[]
            {
                new TestCaseData(new MarketOrderTestParameters(BTCUSDT)).SetName("MarketOrder"),
                new TestCaseData(new LimitOrderTestParameters(BTCUSDT, 50000m, 10000m)).SetName("LimitOrder"),
                new TestCaseData(new StopMarketOrderTestParameters(BTCUSDT, 50000m, 10000m)).SetName("StopMarketOrder"),
                new TestCaseData(new StopLimitOrderTestParameters(BTCUSDT, 50000m, 10000m)).SetName("StopLimitOrder"),
                new TestCaseData(new LimitIfTouchedOrderTestParameters(BTCUSDT, 50000m, 20000)).SetName(
                    "LimitIfTouchedOrder")
            };
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void CancelOrders(OrderTestParameters parameters)
        {
            base.CancelOrders(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void LongFromZero(OrderTestParameters parameters)
        {
            base.LongFromZero(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void CloseFromLong(OrderTestParameters parameters)
        {
            base.CloseFromLong(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void ShortFromZero(OrderTestParameters parameters)
        {
            base.ShortFromZero(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void CloseFromShort(OrderTestParameters parameters)
        {
            base.CloseFromShort(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void ShortFromLong(OrderTestParameters parameters)
        {
            base.ShortFromLong(parameters);
        }

        [Test, TestCaseSource(nameof(OrderParameters))]
        public override void LongFromShort(OrderTestParameters parameters)
        {
            base.LongFromShort(parameters);
        }
        
    }
}