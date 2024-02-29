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

using NUnit.Framework;
using QuantConnect.ToolBox;
using QuantConnect.Util;
using System;
using System.Linq;

namespace QuantConnect.Brokerages.Bybit.Tests
{
    [TestFixture, Explicit("Requires valid credentials to be setup and run outside USA")]
    public class BybitExchangeInfoDownloaderTests
    {
        [Test]
        public void GetsExchangeInfo()
        {
            var eid = Composer.Instance.GetExportedValueByTypeName<IExchangeInfoDownloader>("BybitExchangeInfoDownloader");
            var tickers = eid.Get().ToList();
            
            Assert.IsTrue(tickers.Any());
            foreach (var tickerLine in tickers)
            {
                Assert.IsTrue(tickerLine.StartsWith(eid.Market, StringComparison.OrdinalIgnoreCase));
                var data = tickerLine.Split(",");
                Assert.AreEqual(10, data.Length);
                
                Assert.IsFalse(data[1].StartsWith("10"));
            }
        }
    }
}