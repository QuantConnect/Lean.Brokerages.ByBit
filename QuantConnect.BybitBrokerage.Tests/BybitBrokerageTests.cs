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
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Securities;
using QuantConnect.Tests.Brokerages;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage.Tests
{
    [TestFixture]
    public partial class BybitBrokerageTests : BrokerageTests
    {
        protected static Symbol BTCUSDT = Symbol.Create("BTCUSDT", SecurityType.Crypto, "bybit");
        private BybitApi _client;
        protected override Symbol Symbol { get; } = BTCUSDT;
        protected override SecurityType SecurityType => SecurityType.Crypto;

        protected override decimal GetDefaultQuantity() => 0.0005m;

        protected override bool IsAsync() => false;
        // protected override bool IsCancelAsync() => true;


        protected virtual ISymbolMapper SymbolMapper => new SymbolPropertiesDatabaseSymbolMapper(Market.Bybit);

        

        protected override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
        {
            var algorithm = new Mock<IAlgorithm>();

            var apiKey = Config.Get("bybit-api-key");
            var apiSecret = Config.Get("bybit-api-secret");
            var apiUrl = Config.Get("bybit-api-url", "https://api-testnet.bybit.com");
            var websocketUrl = Config.Get("bybit-websocket-url", "wss://stream-testnet.bybit.com");

            _client = CreateRestApiClient(apiKey, apiSecret, apiUrl);
            return new BybitBrokerage(apiKey, apiSecret, apiUrl, websocketUrl, algorithm.Object, orderProvider,
                securityProvider, new AggregationManager(), null, Market.Bybit, 50);
        }

        protected virtual BybitApi CreateRestApiClient(string apiKey, string apiSecret, string apiUrl)
        {
            return new BybitApi(SymbolMapper, null, apiKey, apiSecret, apiUrl);
        }


        protected override decimal GetAskPrice(Symbol symbol)
        {
            var brokerageSymbol = SymbolMapper.GetBrokerageSymbol(symbol);
            return _client.Market.GetTicker(BybitProductCategory.Spot, brokerageSymbol).Ask1Price!.Value;
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

        [Test,
         Explicit("This test requires reading the output and selection of a low volume security for the Brokerage")]
        public void PartialFillsAndCancelsWhenMarket()
        {
            var manualResetEvent = new ManualResetEvent(false);

            var qty = GetDefaultQuantity() * 50;
            var remaining = qty;
            var sync = new object();
            Brokerage.OrdersStatusChanged += (sender, orderEvents) =>
            {
                lock (sync)
                {
                    var orderEvent = orderEvents[0];
                    remaining -= orderEvent.FillQuantity;
                    Log.Trace("Remaining: " + remaining + " FillQuantity: " + orderEvent.FillQuantity);
                    if (orderEvent.Status == Orders.OrderStatus.Filled)
                    {
                        manualResetEvent.Set();
                    }
                }
            };

            // pick a security with low, but some, volume
            var symbol = BTCUSDT;
            var order = new MarketOrder(symbol, qty, DateTime.UtcNow);
            OrderProvider.Add(order);
            Brokerage.PlaceOrder(order);

            // pause for a while to wait for fills to come in
            manualResetEvent.WaitOne(2500);
            manualResetEvent.WaitOne(2500);
            manualResetEvent.WaitOne(2500);

            Log.Trace("Remaining: " + remaining);
            Assert.AreEqual(0, remaining);
        }

        [Test]
        public override void GetAccountHoldings()
        {
            Log.Trace("");
            Log.Trace("GET ACCOUNT HOLDINGS");
            Log.Trace("");
            var before = Brokerage.GetCashBalance();

            var order = new MarketOrder(Symbol, GetDefaultQuantity(), DateTime.UtcNow);
            PlaceOrderWaitForStatus(order);

            Thread.Sleep(3000);

            var after = Brokerage.GetCashBalance();

            CurrencyPairUtil.DecomposeCurrencyPair(Symbol, out var baseCurrency, out _);
            var beforeHoldings = before.FirstOrDefault(x => x.Currency == baseCurrency);
            var afterHoldings = after.FirstOrDefault(x => x.Currency == baseCurrency);

            var beforeQuantity = beforeHoldings == null ? 0 : beforeHoldings.Amount;
            var afterQuantity = afterHoldings == null ? 0 : afterHoldings.Amount;

            var fee = order.Quantity * BybitFeeModel.TakerNonVIPFee;

            Assert.AreEqual(GetDefaultQuantity(), afterQuantity - beforeQuantity + fee);
        }

        protected override void ModifyOrderUntilFilled(Order order, OrderTestParameters parameters,
            double secondsTimeout = 90)
        {
            Assert.Pass("Order update not supported. Please cancel and re-create.");
        }
    }
}