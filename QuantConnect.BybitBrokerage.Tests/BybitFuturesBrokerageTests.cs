using Moq;
using NUnit.Framework;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Securities;
using QuantConnect.Tests.Brokerages;

namespace QuantConnect.BybitBrokerage.Tests;


[TestFixture, Explicit("Requires valid credentials to be setup and run outside USA")]
public partial class BybitFuturesBrokerageTests : BrokerageTests
{
    protected static Symbol BTCUSDT = Symbol.Create("BTCUSDT", SecurityType.CryptoFuture, "bybit");
    private BybitApi _client;
    protected override Symbol Symbol { get; } = BTCUSDT;
    protected override SecurityType SecurityType => SecurityType.Future;
    protected virtual ISymbolMapper SymbolMapper => new SymbolPropertiesDatabaseSymbolMapper(Market.Bybit);
    protected override decimal GetDefaultQuantity() => 0.001m;

    protected override IBrokerage CreateBrokerage(IOrderProvider orderProvider, ISecurityProvider securityProvider)
    {
        var algorithm = new Mock<IAlgorithm>();
        var apiKey = Config.Get("bybit-api-key");
        var apiSecret = Config.Get("bybit-api-secret");
        var apiUrl = Config.Get("bybit-api-url", "https://api-testnet.bybit.com");
        var websocketUrl = Config.Get("bybit-websocket-url", "wss://stream-testnet.bybit.com");

        _client = CreateRestApiClient(apiKey, apiSecret, apiUrl);
        return new BybitBrokerage(apiKey, apiSecret, apiUrl, websocketUrl, algorithm.Object, orderProvider,
            securityProvider, new AggregationManager(), null, Market.Bybit);
    }

    protected virtual BybitApi CreateRestApiClient(string apiKey, string apiSecret, string apiUrl)
    {
        return new BybitApi(SymbolMapper, null, apiKey, apiSecret, apiUrl);
    }


    protected override bool IsAsync() => false;

    protected override decimal GetAskPrice(Symbol symbol)
    {
        var brokerageSymbol = SymbolMapper.GetBrokerageSymbol(symbol);
        return _client.Market.GetTicker(BybitProductCategory.Linear, brokerageSymbol).Ask1Price!.Value;
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

    private static Symbol[] InverseSymbols()
    {
        return new[]
        {
            QuantConnect.Symbol.Create("BTCUSD", SecurityType.CryptoFuture, Market.Bybit),
            QuantConnect.Symbol.Create("BTCUSDZ23", SecurityType.CryptoFuture, Market.Bybit)
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

    [Test, TestCaseSource(nameof(InverseSymbols))]
    public void InversePairNotSupported(Symbol symbol)
    {
        var parameter = new MarketOrderTestParameters(symbol);
        var order = parameter.CreateLongOrder(GetDefaultQuantity());

        BrokerageMessageEvent brokerageMessageEvent = null;

        void OnBrokerageOnMessage(object sender, BrokerageMessageEvent @event)
        {
            brokerageMessageEvent = @event;
            Brokerage.Message -= OnBrokerageOnMessage;
        }

        Brokerage.Message += OnBrokerageOnMessage;

        Assert.IsFalse(Brokerage.PlaceOrder(order));
        Assert.IsNotNull(brokerageMessageEvent);
        Assert.AreEqual($"Symbol is not supported {order.Symbol}", brokerageMessageEvent.Message);
    }
}