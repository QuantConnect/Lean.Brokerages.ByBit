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
using QuantConnect.Configuration;
using QuantConnect.Util;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Securities;
using QuantConnect.Tests.Common.Securities;

namespace QuantConnect.BybitBrokerage.Tests
{
    [TestFixture]
    public class BybitBrokerageAdditionalTests
    {

        protected virtual string BrokerageName => nameof(BybitBrokerage);
        
        [Test]
        public void ParameterlessConstructorComposerUsage()
        {
            var brokerage = Composer.Instance.GetExportedValueByTypeName<IDataQueueHandler>(BrokerageName);
            Assert.IsNotNull(brokerage);
        }

        [Test]
        public void ConnectedIfNoAlgorithm()
        {
            using var brokerage = CreateBrokerage(null);
            Assert.True(brokerage.IsConnected);
        }

        [Test]
        public void ConnectionFailsIfAuthenticationFails()
        {
            var securities = new SecurityManager(new TimeKeeper(DateTime.UtcNow, TimeZones.Utc));
            var algorithmSettings = new AlgorithmSettings();
            var transactions = new SecurityTransactionManager(null, securities);
            transactions.SetOrderProcessor(new FakeOrderProcessor());

            var algorithm = new Mock<IAlgorithm>();
            algorithm.Setup(a => a.Transactions).Returns(transactions);
            algorithm.Setup(a => a.BrokerageModel).Returns(new BinanceBrokerageModel());
            algorithm.Setup(a => a.Portfolio).Returns(new SecurityPortfolioManager(securities, transactions, algorithmSettings));

            using var brokerage = CreateBrokerage(algorithm.Object, true);
            
            //this should throw while connecting to the private WS NOT in the GetCashBalance function
            var testDelegate = new TestDelegate(() => brokerage.GetCashBalance());
            Assert.Throws<Exception>(testDelegate);
        }

        [Test]
        public void ConnectedIfAlgorithmIsNotNullAndClientNotCreated()
        {
            using var brokerage = CreateBrokerage(Mock.Of<IAlgorithm>());
            Assert.True(brokerage.IsConnected);
        }

        [Test]
        public void ConnectToUserDataStreamIfAlgorithmNotNullAndApiIsCreated()
        {
            var securities = new SecurityManager(new TimeKeeper(DateTime.UtcNow, TimeZones.Utc));
            var algorithmSettings = new AlgorithmSettings();
            var transactions = new SecurityTransactionManager(null, securities);
            transactions.SetOrderProcessor(new FakeOrderProcessor());

            var algorithm = new Mock<IAlgorithm>();
            algorithm.Setup(a => a.Transactions).Returns(transactions);
            algorithm.Setup(a => a.BrokerageModel).Returns(new BinanceBrokerageModel());
            algorithm.Setup(a => a.Portfolio).Returns(new SecurityPortfolioManager(securities, transactions, algorithmSettings));

            using var brokerage = CreateBrokerage(algorithm.Object);

            Assert.True(brokerage.IsConnected);

            var _ = brokerage.GetCashBalance();

            Assert.True(brokerage.IsConnected);

            brokerage.Disconnect();

            Assert.False(brokerage.IsConnected);
        }
        
        private Brokerage CreateBrokerage(IAlgorithm algorithm, bool noSecrets = false)
        {
            var apiKey = noSecrets ? string.Empty : Config.Get("bybit-api-key");
            var apiSecret = noSecrets ? string.Empty : Config.Get("bybit-api-secret");
            var apiUrl = Config.Get("bybit-api-url", "https://api-testnet.bybit.com");
            var websocketUrl = Config.Get("bybit-websocket-url", "wss://stream-testnet.bybit.com");

            return CreateBrokerage(algorithm, apiKey, apiSecret, apiUrl, websocketUrl);
        }

        protected virtual Brokerage CreateBrokerage(IAlgorithm algorithm, string apiKey, string apiSecret,
            string apiUrl, string websocketUrl)
        {
            return new BybitBrokerage(apiKey, apiSecret, apiUrl, websocketUrl, algorithm,new AggregationManager(),null);
        }
    }
}