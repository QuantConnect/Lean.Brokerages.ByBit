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
using QuantConnect.Lean.Engine.HistoricalData;

namespace QuantConnect.BybitBrokerage.Tests
{
    [TestFixture]
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
                return new[]
                {
                    // valid
                    new TestCaseData(ETHUSDT, Resolution.Tick, Time.OneMinute, TickType.Trade, false),
                    new TestCaseData(ETHUSDT, Resolution.Minute, Time.OneHour, TickType.Trade, false),
                    new TestCaseData(ETHUSDT, Resolution.Hour, Time.OneDay, TickType.Trade, false),
                    new TestCaseData(ETHUSDT, Resolution.Daily, TimeSpan.FromDays(15), TickType.Trade, false),
                };
            }
        }

        private static TestCaseData[] NoHistory
        {
            get
            {
                return new[]
                {
                    new TestCaseData(Symbol.Create("ETHUSDT", SecurityType.Crypto, Market.Bybit),
                        Resolution.Second, Time.OneMinute, TickType.Trade),
                    new TestCaseData(Symbol.Create("ETHUSDT", SecurityType.Crypto, Market.Bybit),
                        Resolution.Minute, Time.OneHour, TickType.Quote),
                };
            }
        }

        private static TestCaseData[] InvalidHistory
        {
            get
            {
                return new[]
                {
                    // invalid period, no error, empty result
                    new TestCaseData(Symbols.EURUSD, Resolution.Daily, TimeSpan.FromDays(-15), TickType.Trade),

                    // invalid symbol, throws "System.ArgumentException : Unknown symbol: XYZ"
                    new TestCaseData(Symbol.Create("XYZ", SecurityType.CryptoFuture, Market.Bybit), Resolution.Daily,
                        TimeSpan.FromDays(15), TickType.Trade),

                    //invalid security type
                    new TestCaseData(Symbols.AAPL, Resolution.Daily, TimeSpan.FromDays(15), TickType.Trade),
                };
            }
        }


        [Test]
        [TestCaseSource(nameof(ValidHistory))]
        public virtual void GetsHistory(Symbol symbol, Resolution resolution, TimeSpan period, TickType tickType,
            bool throwsException)
        {
            BaseHistoryTest(_brokerage, symbol, resolution, period, tickType, throwsException, false);
        }

        [Test]
        [TestCaseSource(nameof(NoHistory))]
        [TestCaseSource(nameof(InvalidHistory))]
        public virtual void GetEmptyHistory(Symbol symbol, Resolution resolution, TimeSpan period, TickType tickType)
        {
            BaseHistoryTest(_brokerage, symbol, resolution, period, tickType, false, true);
        }

        protected static void BaseHistoryTest(Brokerage brokerage, Symbol symbol, Resolution resolution,
            TimeSpan period, TickType tickType, bool throwsException, bool emptyData)
        {
            TestDelegate test = () =>
            {
                var historyProvider = new BrokerageHistoryProvider();
                historyProvider.SetBrokerage(brokerage);
                historyProvider.Initialize(new HistoryProviderInitializeParameters(null, null, null,
                    null, null, null, null,
                    false, new DataPermissionManager()));

                var marketHoursDatabase = MarketHoursDatabase.FromDataFolder();

                var now = DateTime.UtcNow.AddDays(-1);
                var requests = new[]
                {
                    new HistoryRequest(now.Add(-period),
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
                        tickType)
                };
                
                var historyArray = historyProvider.GetHistory(requests, TimeZones.Utc).ToArray();
                foreach (var slice in historyArray)
                {
                    if (resolution == Resolution.Tick)
                    {
                        foreach (var tick in slice.Ticks[symbol])
                        {
                            Log.Trace("{0}: {1} - {2} / {3}", tick.Time.ToStringInvariant("yyyy-MM-dd HH:mm:ss.fff"),
                                tick.Symbol, tick.BidPrice, tick.AskPrice);
                        }
                    }
                    else if (slice.QuoteBars.TryGetValue(symbol, out var quoteBar))
                    {
                        Log.Trace($"QuoteBar: {quoteBar}");
                    }
                    else if (slice.Bars.TryGetValue(symbol, out var bar))
                    {
                        Log.Trace("{0}: {1} - O={2}, H={3}, L={4}, C={5}", bar.Time, bar.Symbol, bar.Open, bar.High,
                            bar.Low, bar.Close);
                    }
                }

                if (emptyData)
                {
                    Assert.Zero(historyProvider.DataPointCount);
                }
                else
                {
                    Assert.Greater(historyProvider.DataPointCount, 0);
                }

                if (historyProvider.DataPointCount > 0)
                {
                    // Ordered by time
                    Assert.That(historyArray, Is.Ordered.By("Time"));

                    // No repeating bars
                    var timesArray = historyArray.Select(x => x.Time).ToArray();
                    Assert.AreEqual(timesArray.Length, timesArray.Distinct().Count());


                    // No missing bars
                    if (resolution != Resolution.Tick && historyProvider.DataPointCount >= 2)
                    {
                        var diff = resolution.ToTimeSpan();
                        for (var i = 1; i < timesArray.Length; i++)
                        {
                            Assert.AreEqual(diff, timesArray[i] - timesArray[i - 1]);
                        }
                    }
                }

                Log.Trace("Data points retrieved: " + historyProvider.DataPointCount);
            };

            if (throwsException)
            {
                Assert.Throws<ArgumentException>(test);
            }
            else
            {
                Assert.DoesNotThrow(test);
            }
        }

        protected virtual Brokerage CreateBrokerage()
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
                null,
                50
            );
        }
    }
}