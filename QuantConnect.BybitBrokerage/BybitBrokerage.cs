using System;
using System.Collections.Generic;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;
using HistoryRequest = QuantConnect.Data.HistoryRequest;
using OrderStatus = QuantConnect.Orders.OrderStatus;

namespace QuantConnect.BybitBrokerage;

//todo margin
//todo open interest?
//todo funding rate?
[BrokerageFactory(typeof(BybitBrokerageFactory))]
public partial class BybitBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
{
    private IAlgorithm _algorithm;
    private SymbolPropertiesDatabaseSymbolMapper _symbolMapper;
    private LiveNodePacket _job;
    private string _privateWebSocketUrl;
    private Lazy<BybitApi> _apiClientLazy;

    private BrokerageConcurrentMessageHandler<WebSocketMessage> _messageHandler;

    protected string MarketName { get; set; }
    protected BybitApi ApiClient => _apiClientLazy?.Value;
    protected IOrderProvider OrderProvider { get; private set; }
    protected virtual BybitAccountCategory Category => BybitAccountCategory.Spot;

    /// <summary>
    /// Returns true if we're currently connected to the broker
    /// </summary>
    public override bool IsConnected => _apiClientLazy?.IsValueCreated != true || WebSocket?.IsOpen == true;

    /// <summary>
    /// Parameterless constructor for brokerage
    /// </summary>
    public BybitBrokerage() : this(Market.Bybit)
    {
    }

    /// <summary>
    /// Constructor for brokerage
    /// </summary>
    public BybitBrokerage(string marketName) : base(marketName)
    {
    }


    /// <summary>
    /// Constructor for brokerage
    /// </summary>
    /// <param name="apiKey">api key</param>
    /// <param name="apiSecret">api secret</param>
    /// <param name="restApiUrl">The rest api url</param>
    /// <param name="webSocketBaseUrl">The web socket base url</param>
    /// <param name="aggregator">the aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    /// <param name="marketName">Actual market name</param>
    public BybitBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IOrderProvider orderProvider, ISecurityProvider securityProvider, IDataAggregator aggregator,
        LiveNodePacket job, string marketName = Market.Bybit)
        : this(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, null, orderProvider, securityProvider, aggregator, job,
            marketName)
    {
    }

    /// <summary>
    /// Constructor for brokerage
    /// </summary>
    /// <param name="apiKey">api key</param>
    /// <param name="apiSecret">api secret</param>
    /// <param name="restApiUrl">The rest api url</param>
    /// <param name="webSocketBaseUrl">The web socket base url</param>
    /// <param name="algorithm">the algorithm instance is required to retrieve account type</param>
    /// <param name="aggregator">the aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    public BybitBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job)
        : this(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, algorithm, algorithm?.Portfolio?.Transactions,
            algorithm?.Portfolio, aggregator, job, Market.Bybit)
    {
    }

    /// <summary>
    /// Constructor for brokerage
    /// </summary>
    /// <param name="apiKey">api key</param>
    /// <param name="apiSecret">api secret</param>
    /// <param name="restApiUrl">The rest api url</param>
    /// <param name="webSocketBaseUrl">The web socket base url</param>
    /// <param name="algorithm">the algorithm instance is required to retrieve account type</param>
    /// <param name="aggregator">the aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    /// <param name="orderProvider"></param>
    /// <param name="marketName">Actual market name</param>
    public BybitBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IOrderProvider orderProvider, ISecurityProvider securityProvider,
        IDataAggregator aggregator, LiveNodePacket job, string marketName)
        : base(marketName)
    {
        Initialize(
            webSocketBaseUrl,
            restApiUrl,
            apiKey,
            apiSecret,
            algorithm,
            orderProvider,
            securityProvider,
            aggregator,
            job,
            marketName
        );
    }


    public override IEnumerable<BaseData> GetHistory(HistoryRequest request)
    {
        if (!_symbolMapper.IsKnownLeanSymbol(request.Symbol))
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidSymbol",
                $"Unknown symbol: {request.Symbol.Value}, no history returned"));
            yield break;
        }

        if (request.Resolution == Resolution.Second)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidResolution",
                $"{request.Resolution} resolution is not supported, no history returned"));
            yield break;
        }

        if (request.TickType != TickType.Trade)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidTickType",
                $"{request.TickType} tick type not supported, no history returned"));
            yield break;
        }

        if (request.Symbol.SecurityType != GetSupportedSecurityType())
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidSecurityType",
                $"{request.Symbol.SecurityType} security type not supported, no history returned"));
            yield break;
        }

        var brokerageSymbol = _symbolMapper.GetBrokerageSymbol(request.Symbol);

        if (request.Resolution == Resolution.Tick)
        {
            var res = new BybitArchiveDownloader().Download(Category, brokerageSymbol, request.StartTimeUtc,
                request.EndTimeUtc);
            foreach (var tick in res)
            {
                yield return new Tick(tick.Time, request.Symbol, string.Empty, MarketName,
                    tick.Size * (tick.Side == OrderSide.Buy ? 1m : -1m), tick.price);
            }

            yield break;
        }

        var client = ApiClient ??
                     GetApiClient(_symbolMapper, null, Config.Get("bybit-api-url", "https://api.bybit.com"), null,
                         null);

        var kLines = client.Market
            .GetKLines(Category, brokerageSymbol, request.Resolution, request.StartTimeUtc, request.EndTimeUtc);

        var periodTimeSpan = request.Resolution.ToTimeSpan();
        foreach (var byBitKLine in kLines)
        {
            yield return new TradeBar(Time.UnixMillisecondTimeStampToDateTime(byBitKLine.OpenTime), request.Symbol,
                byBitKLine.Open, byBitKLine.High, byBitKLine.Low, byBitKLine.Close, byBitKLine.Volume,
                periodTimeSpan);
        }
    }

    protected void Initialize(string baseWssUrl, string restApiUrl, string apiKey, string apiSecret,
        IAlgorithm algorithm, IOrderProvider orderProvider, ISecurityProvider securityProvider,
        IDataAggregator aggregator, LiveNodePacket job,
        string marketName)
    {
        if (IsInitialized)
        {
            return;
        }

        _privateWebSocketUrl = $"{baseWssUrl}/v5/private";
        var publicWssUrl = $"{baseWssUrl}/v5/public/{Category.ToStringInvariant().ToLowerInvariant()}";

        base.Initialize(_privateWebSocketUrl, new BybitWebSocketWrapper(), null, apiKey, apiSecret);

        _job = job;
        _algorithm = algorithm;
        _aggregator = aggregator;
        _messageHandler = new BrokerageConcurrentMessageHandler<WebSocketMessage>(OnUserMessage);
        _symbolMapper = new(marketName);
        OrderProvider = orderProvider;
        MarketName = marketName;

        var subscriptionManager = new BrokerageMultiWebSocketSubscriptionManager(publicWssUrl,
            100,
            100,
            new Dictionary<Symbol, int>(),
            () => new BybitWebSocketWrapper(),
            Subscribe,
            Unsubscribe,
            OnDataMessage,
            TimeSpan.FromDays(1));
        SubscriptionManager = subscriptionManager;

        // can be null, if BybitBrokerage is used as DataQueueHandler only
        if (_algorithm != null)
        {
            _apiClientLazy = new Lazy<BybitApi>(() =>
            {
                var client = GetApiClient(_symbolMapper, securityProvider, restApiUrl, apiKey, apiSecret);
                Connect(client);
                return client;
            });
        }
        //todo ValidateSubscription(); 
    }

    protected virtual bool CanSubscribe(Symbol symbol)
    {
        return !symbol.Value.Contains("UNIVERSE") &&
               symbol.SecurityType == GetSupportedSecurityType() &&
               symbol.ID.Market == MarketName;
    }

    protected virtual SecurityType GetSupportedSecurityType()
    {
        return SecurityType.Crypto;
    }


    /// <summary>
    /// Adds the specified symbols to the subscription
    /// </summary>
    /// <param name="symbols">The symbols to be added keyed by SecurityType</param>
    protected override bool Subscribe(IEnumerable<Symbol> symbols)
    {
        // NOP
        return true;
    }

    protected virtual BybitApi GetApiClient(ISymbolMapper symbolMapper, ISecurityProvider securityProvider,
        string restApiUrl, string apiKey, string apiSecret)
    {
        var url = Config.Get("bybit-api-url", "https://api.bybit.com");
        return new BybitApi(symbolMapper, securityProvider, apiKey, apiSecret, restApiUrl);
    }

    public override void Dispose()
    {
        SubscriptionManager.DisposeSafely();
        ApiClient.DisposeSafely();


        base.Dispose();
    }

    private static OrderStatus ConvertOrderStatus(Models.Enums.OrderStatus orderStatus)
    {
        switch (orderStatus)
        {
            //todo verify especially triggered/untriggered
            case Models.Enums.OrderStatus.Created:
            case Models.Enums.OrderStatus.New:
            case Models.Enums.OrderStatus.Untriggered:
            case Models.Enums.OrderStatus.Triggered:
            case Models.Enums.OrderStatus.Active:
                return OrderStatus.Submitted;
            case Models.Enums.OrderStatus.PartiallyFilled:
                return OrderStatus.PartiallyFilled;
            case Models.Enums.OrderStatus.Filled:
                return OrderStatus.Filled;
            case Models.Enums.OrderStatus.Cancelled:
            case Models.Enums.OrderStatus.Deactivated:
            case Models.Enums.OrderStatus.PartiallyFilledCanceled:
                return OrderStatus.Canceled;
            case Models.Enums.OrderStatus.Rejected:
                return OrderStatus.Invalid;
            default:
                throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null);
        }
    }
}