﻿/*
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
using NUnit.Framework;

namespace QuantConnect.BybitBrokerage.Tests
{
    [TestFixture, Explicit("Requires valid credentials to be setup and run outside USA")]
    public class BybitFuturesBrokerageHistoryProviderTests : BybitBrokerageHistoryProviderTests
    {
        private static readonly Symbol ETHUSDT = Symbol.Create("ETHUSDT", SecurityType.CryptoFuture, Market.Bybit);


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
                    new TestCaseData(ETHUSDT, Resolution.Hour, Time.OneDay, TickType.OpenInterest, false)
                };
            }
        }


        [Test, TestCaseSource(nameof(ValidHistory))]
        public override void GetsHistory(Symbol symbol, Resolution resolution, TimeSpan period, TickType tickType,
            bool throwsException)
        {
            base.GetsHistory(symbol, resolution, period, tickType, throwsException);
        }

        [Ignore("Same as base")]
        public override void GetEmptyHistory(Symbol symbol, Resolution resolution, TimeSpan period, TickType tickType,
            bool throwsException)
        {
            base.GetEmptyHistory(symbol, resolution, period, tickType, throwsException);
        }
    }
}