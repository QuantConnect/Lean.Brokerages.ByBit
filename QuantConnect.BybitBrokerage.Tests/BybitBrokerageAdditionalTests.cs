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
using QuantConnect.Brokerages;
using QuantConnect.Configuration;
using QuantConnect.Util;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;

namespace QuantConnect.BybitBrokerage.Tests
{
    [TestFixture]
    public class BybitBrokerageAdditionalTests
    {
        [Test]
        public void ParameterlessConstructorComposerUsage()
        {
            var brokerage = Composer.Instance.GetExportedValueByTypeName<IDataQueueHandler>("BybitBrokerage");
            Assert.IsNotNull(brokerage);
        }

        [Test]
        public void ConnectedIfNoAlgorithm()
        {
            using var brokerage = CreateBrokerage(null);
            Assert.True(brokerage.IsConnected);
        }

        protected virtual Brokerage CreateBrokerage(IAlgorithm algorithm)
        {
            var apiKey = Config.Get("bybit-api-key");
            var apiSecret = Config.Get("bybit-api-secret");
            var apiUrl = Config.Get("bybit-api-url", "https://api-testnet.bybit.com");
            var websocketUrl = Config.Get("bybit-websocket-url", "wss://stream-testnet.bybit.com");
            
            return new BybitFuturesBrokerage(apiKey, apiSecret, apiUrl, websocketUrl, algorithm,new AggregationManager(),null);
        }
    }
}