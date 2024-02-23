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
using NUnit.Framework;
using QuantConnect.Brokerages;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Tests;
using QuantConnect.Logging;
using QuantConnect.Securities;
using QuantConnect.Data.Market;
using QuantConnect.Lean.Engine.DataFeeds;

namespace QuantConnect.BybitBrokerage.Tests
{
    [TestFixture, Explicit("Requires valid credentials to be setup and run outside USA")]
    public class BybitBrokerageHistoryProviderTests
    {
        private static readonly Symbol ETHUSDT = Symbol.Create("ETHUSDT", SecurityType.Crypto, Market.Bybit);
        private Brokerage _brokerage;

        [OneTimeSetUp]
        public void Setup()
        {
            _brokerage = CreateBrokerage();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _brokerage?.Disconnect();
            _brokerage?.Dispose();
        }

        private static TestCaseData[] ValidHistory
        {
            get
            {
                TestGlobals.Initialize();

                return new[]
                {
                    // valid
                    new TestCaseData(ETHUSDT, Resolution.Tick, Time.OneMinute, TickType.Trade),
                    new TestCaseData(ETHUSDT, Resolution.Minute, Time.OneHour, TickType.Trade),
                    new TestCaseData(ETHUSDT, Resolution.Hour, Time.OneDay, TickType.Trade),
                    new TestCaseData(ETHUSDT, Resolution.Daily, TimeSpan.FromDays(15), TickType.Trade),
                };
            }
        }

        private static TestCaseData[] InvalidHistory
        {
            get
            {
                return new[]
                {
                    // invalid period
                    new TestCaseData(ETHUSDT, Resolution.Daily, TimeSpan.FromDays(-15), TickType.Trade),

                    // invalid symbol
                    new TestCaseData(Symbol.Create("XYZ", SecurityType.CryptoFuture, Market.Bybit), Resolution.Daily,
                        TimeSpan.FromDays(15), TickType.Trade),

                    //invalid security type
                    new TestCaseData(Symbols.AAPL, Resolution.Daily, TimeSpan.FromDays(15), TickType.Trade),

                    // invalid resolution
                    new TestCaseData(ETHUSDT, Resolution.Second, Time.OneMinute, TickType.Trade),

                    // invalid tick type
                    new TestCaseData(ETHUSDT, Resolution.Minute, Time.OneHour, TickType.Quote),

                    // invalid market
                    new TestCaseData(Symbol.Create("ETHUSDT", SecurityType.Crypto, Market.Binance), Resolution.Minute,
                        Time.OneMinute, TickType.Trade),

                    // invalid resolution for tick type
                    new TestCaseData(ETHUSDT, Resolution.Tick, TimeSpan.FromDays(15), TickType.OpenInterest),
                    new TestCaseData(ETHUSDT, Resolution.Minute, TimeSpan.FromDays(15), TickType.OpenInterest),
                };
            }
        }

        [Test]
        [TestCaseSource(nameof(ValidHistory))]
        public virtual void GetsHistory(Symbol symbol, Resolution resolution, TimeSpan period, TickType tickType)
        {
            BaseHistoryTest(_brokerage, symbol, resolution, period, tickType, false);
        }

        [Test]
        [TestCaseSource(nameof(InvalidHistory))]
        public virtual void ReturnsNullOnInvalidHistoryRequest(Symbol symbol, Resolution resolution, TimeSpan period, TickType tickType)
        {
            BaseHistoryTest(_brokerage, symbol, resolution, period, tickType, true);
        }

        protected static void BaseHistoryTest(Brokerage brokerage, Symbol symbol, Resolution resolution,
            TimeSpan period, TickType tickType, bool invalidRequest)
        {
            var marketHoursDatabase = MarketHoursDatabase.FromDataFolder();

            var now = DateTime.UtcNow.AddDays(-1);
            var request = new HistoryRequest(now.Add(-period),
                now,
                resolution == Resolution.Tick ? typeof(Tick) : typeof(TradeBar),
                symbol,
                resolution,
                marketHoursDatabase.GetExchangeHours(symbol.ID.Market, symbol, symbol.SecurityType),
                marketHoursDatabase.GetDataTimeZone(symbol.ID.Market, symbol, symbol.SecurityType),
                resolution,
                false,
                false,
                DataNormalizationMode.Adjusted,
                tickType);

            var history = brokerage.GetHistory(request)?.ToList();

            if (invalidRequest)
            {
                Assert.IsNull(history);
                return;
            }

            Assert.IsNotNull(history);

            foreach (var data in history)
            {
                if (data is Tick tick)
                {
                    Log.Trace("{0}: {1} - {2} / {3}", tick.Time.ToStringInvariant("yyyy-MM-dd HH:mm:ss.fff"),
                        tick.Symbol, tick.BidPrice, tick.AskPrice);
                }
                else if (data is QuoteBar quoteBar)
                {
                    Log.Trace($"QuoteBar: {quoteBar}");
                }
                else if (data is TradeBar bar)
                {
                    Log.Trace("{0}: {1} - O={2}, H={3}, L={4}, C={5}", bar.Time, bar.Symbol, bar.Open, bar.High,
                        bar.Low, bar.Close);
                }
            }

            Assert.Greater(history.Count, 0);

            // Ordered by time
            Assert.That(history, Is.Ordered.By("Time"));

            var timesArray = history.Select(x => x.Time).ToArray();
            if (resolution != Resolution.Tick)
            {
                // No repeating bars
                Assert.AreEqual(timesArray.Length, timesArray.Distinct().Count());
            }

            foreach (var data in history)
            {
                Assert.AreEqual(symbol, data.Symbol);

                if (data.DataType != MarketDataType.Tick)
                {
                    Assert.AreEqual(resolution.ToTimeSpan(), data.EndTime - data.Time);
                }
            }

            // No missing bars
            if (resolution != Resolution.Tick && history.Count >= 2)
            {
                var diff = resolution.ToTimeSpan();
                for (var i = 1; i < timesArray.Length; i++)
                {
                    Assert.AreEqual(diff, timesArray[i] - timesArray[i - 1]);
                }
            }

            Log.Trace("Data points retrieved: " + history.Count);
        }

        private Brokerage CreateBrokerage()
        {
            var apiKey = Config.Get("bybit-api-key");
            var apiSecret = Config.Get("bybit-api-secret");
            var apiUrl = Config.Get("bybit-api-url", "https://api-testnet.bybit.com");
            var websocketUrl = Config.Get("bybit-websocket-url", "wss://stream-testnet.bybit.com");

            return new BybitBrokerage(
                apiKey,
                apiSecret,
                apiUrl,
                websocketUrl,
                null,
                new AggregationManager(),
                null
            );
        }
    }
}