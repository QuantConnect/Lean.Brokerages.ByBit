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
using NUnit.Framework;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Tests.Brokerages;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage.Tests;

[TestFixture, Explicit("Requires valid credentials to be setup and run outside USA")]
public partial class BybitInverseFuturesBrokerageTests : BybitBrokerageTests
{
    private static Symbol BTCUSD = Symbol.Create("BTCUSD", SecurityType.CryptoFuture, "bybit");
    protected override Symbol Symbol { get; } = BTCUSD;

    protected override SecurityType SecurityType => SecurityType.Future;
    protected override BybitProductCategory Category => BybitProductCategory.Inverse;
    protected override decimal TakerFee => 0.0000015m;

    protected override decimal GetDefaultQuantity() => 10m;

    protected override IBrokerage CreateBrokerage(string apiKey, string apiSecret, string apiUrl,
        string websocketUrl, IAlgorithm algorithm, IOrderProvider orderProvider, ISecurityProvider securityProvider,
        IDataAggregator aggregator)
    {
        return new BybitInverseFuturesBrokerage(apiKey, apiSecret, apiUrl, websocketUrl, algorithm, orderProvider, securityProvider, new AggregationManager(), null);

    }
    
    /// <summary>
    /// Provides the data required to test each order type in various cases
    /// </summary>
    private static TestCaseData[] OrderParameters()
    {
        return new[]
        {
            new TestCaseData(new MarketOrderTestParameters(BTCUSD)).SetName("MarketOrder"),
            new TestCaseData(new LimitOrderTestParameters(BTCUSD, 50000m, 10000m)).SetName("LimitOrder"),
            new TestCaseData(new StopMarketOrderTestParameters(BTCUSD, 50000m, 10000m)).SetName("StopMarketOrder"),
            new TestCaseData(new StopLimitOrderTestParameters(BTCUSD, 50000m, 10000m)).SetName("StopLimitOrder"),
            new TestCaseData(new LimitIfTouchedOrderTestParameters(BTCUSD, 50000m, 20000)).SetName(
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

        var fee = 0.00000015m;
            
        Assert.AreEqual(0, afterQuantity - beforeQuantity + fee);
    }

}