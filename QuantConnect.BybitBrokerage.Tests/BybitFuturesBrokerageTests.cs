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
using QuantConnect.Brokerages.Bybit.Models.Enums;
using QuantConnect.Tests.Brokerages;

namespace QuantConnect.Brokerages.Bybit.Tests;

[TestFixture, Explicit("Requires valid credentials to be setup and run outside USA")]
public partial class BybitFuturesBrokerageTests : BybitBrokerageTests
{
    private static Symbol BTCUSDT = Symbol.Create("BTCUSDT", SecurityType.CryptoFuture, "bybit");
    protected override Symbol Symbol { get; } = BTCUSDT;
    protected override SecurityType SecurityType => SecurityType.Future;
    protected override BybitProductCategory Category => BybitProductCategory.Linear;
    protected override decimal GetDefaultQuantity() => 0.001m;


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

    [Ignore("The brokerage is shared between different product categories, therefore this test is only required in the base class")]
    public override void GetAccountHoldings()
    {
        base.GetAccountHoldings();
    }
}