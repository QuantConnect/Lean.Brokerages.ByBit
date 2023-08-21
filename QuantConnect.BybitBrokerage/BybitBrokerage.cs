using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Statistics;
using QuantConnect.Util;
using HistoryRequest = QuantConnect.Data.HistoryRequest;
using OrderType = QuantConnect.BybitBrokerage.Models.Enums.OrderType;
using Timer = System.Timers.Timer;

namespace QuantConnect.BybitBrokerage;

[BrokerageFactory(typeof(BybitBrokerageFactory))]
public partial class BybitBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
{
    private IAlgorithm _algorithm;
    private SymbolPropertiesDatabaseSymbolMapper _symbolMapper;
    private LiveNodePacket _job;
    private string _webSocketBaseUrl;
    private Timer _keepAliveTimer;

    //private Lazy<BinanceBaseRestApiClient> _apiClientLazy;

    private BrokerageConcurrentMessageHandler<WebSocketMessage> _messageHandler;

    protected string MarketName { get; set; }
    protected BybitRestApiClient ApiClient { get; set; }
    protected IOrderProvider OrderProvider { get; private set; }
    protected virtual BybitAccountCategory Category => BybitAccountCategory.Spot;

    /// <summary>
    /// Returns true if we're currently connected to the broker
    /// </summary>
    public override bool IsConnected => WebSocket?.IsOpen ?? false;

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
       IOrderProvider orderProvider, IDataAggregator aggregator, LiveNodePacket job, string marketName = Market.Bybit)
        : this(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, null, aggregator,job,  orderProvider, marketName)
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
        : this(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, algorithm, aggregator, job,algorithm?.Portfolio?.Transactions, Market.Bybit)
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
        IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job, IOrderProvider orderProvider,
        string marketName)
        : base(marketName)
    {
        Initialize(
            webSocketBaseUrl,
            restApiUrl,
            apiKey,
            apiSecret,
            algorithm,
            orderProvider,
            aggregator,
            job,
            marketName
        );
    }


    protected void Initialize(string wssUrl, string restApiUrl, string apiKey, string apiSecret,
        IAlgorithm algorithm,IOrderProvider orderProvider, IDataAggregator aggregator, LiveNodePacket job, string marketName)
    {
        if (IsInitialized)
        {
            return;
        }

        var privateWssURl = $"{wssUrl}/v5/private";
        wssUrl += $"/v5/public/{Category.ToStringInvariant().ToLowerInvariant()}";

        //todo why here?

        base.Initialize(privateWssURl, new WebSocketClientWrapper(), null, apiKey, apiSecret);

        _job = job;
        _algorithm = algorithm;
        _aggregator = aggregator;
        _webSocketBaseUrl = privateWssURl;
        _messageHandler = new BrokerageConcurrentMessageHandler<WebSocketMessage>(OnUserMessage);
        _symbolMapper = new(marketName);
        OrderProvider = orderProvider;
        MarketName = marketName;

        
        // todo send ping for public 
        var subscriptionManager = new BrokerageMultiWebSocketSubscriptionManager(wssUrl,
            100,
            100,
            new Dictionary<Symbol, int>(),
            () => new BybitWebSocketWrapper(null),
            Subscribe,
            Unsubscribe,
            OnDataMessage,
            TimeSpan.FromDays(1));


        SubscriptionManager = subscriptionManager;
        ApiClient = GetApiClient(_symbolMapper, _algorithm?.Portfolio, restApiUrl, apiKey, apiSecret);

        _keepAliveTimer = new()
        {
            // 20 seconds
            Interval = 20 * 1000,
        };
        _keepAliveTimer.Elapsed += (_, _) => Send(WebSocket, new { op = "ping" }); //todo

        WebSocket.Open += (s, e) =>
        {
            _keepAliveTimer.Start();
            Send(WebSocket,ApiClient.AuthenticateWS());
            Send(WebSocket, new{op="subscribe",args = new[]{"order"}});
        };
        
        WebSocket.Closed += (s, e) =>
        {
            _keepAliveTimer.Stop();
        };

        //ValidateSubscription(); todo
    }

    private bool CanSubscribe(Symbol symbol)
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

        var brokerageSymbol = _symbolMapper.GetBrokerageSymbol(request.Symbol);

        if (request.Resolution == Resolution.Tick)
        {
            var res = new BybitArchiveDownloader().Download(Category, brokerageSymbol, request.StartTimeUtc,
                request.EndTimeUtc); //todo end
            foreach (var tick in res)
            {
                yield return new Tick(tick.Time, request.Symbol, string.Empty, Exchange.Bybit,
                    tick.Size * (tick.Side == OrderSide.Buy ? 1m : -1m), tick.price);
            }

            yield break;
        }

        var client = ApiClient ?? GetApiClient(_symbolMapper, null,
            Config.Get("bybit-api-url", "https://api.bybit.com"), null, null);

        var kLines = client
            .GetKLines(Category, brokerageSymbol, request.Resolution, request.StartTimeUtc, request.EndTimeUtc);

        var periodTimeSpan = request.Resolution.ToTimeSpan();
        foreach (var byBitKLine in kLines)
        {
            yield return new TradeBar(Time.UnixMillisecondTimeStampToDateTime(byBitKLine.OpenTime), request.Symbol,
                byBitKLine.Open, byBitKLine.High, byBitKLine.Low, byBitKLine.Close, byBitKLine.Volume,
                periodTimeSpan);
        }
    }

    protected virtual BybitRestApiClient GetApiClient(ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider, string restApiUrl, string apiKey, string apiSecret)
    {
        var url = Config.Get("bybit-api-url", "https://api.bybit.com");
        return new BybitRestApiClient(symbolMapper, securityProvider, apiKey, apiSecret, restApiUrl);
    }

    public override void Dispose()
    {
        _keepAliveTimer.DisposeSafely();
        _webSocketRateLimiter.DisposeSafely();
        SubscriptionManager.DisposeSafely();

        base.Dispose();
    }
}