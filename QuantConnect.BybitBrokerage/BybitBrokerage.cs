using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
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
using QuantConnect.Util;
using HistoryRequest = QuantConnect.Data.HistoryRequest;
using OrderType = QuantConnect.BybitBrokerage.Models.Enums.OrderType;

namespace QuantConnect.BybitBrokerage;

[BrokerageFactory(typeof(BybitBrokerageFactory))]
public partial class BybitBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
{
    private readonly EventBasedDataQueueHandlerSubscriptionManager _subscriptionManager;


    private IAlgorithm _algorithm;
    private SymbolPropertiesDatabaseSymbolMapper _symbolMapper;



    private long _lastRequestId;

    private LiveNodePacket _job;
    private string _webSocketBaseUrl;
    private Timer _keepAliveTimer;
    private Timer _reconnectTimer;

    //private Lazy<BinanceBaseRestApiClient> _apiClientLazy;

    private BrokerageConcurrentMessageHandler<WebSocketMessage> _messageHandler;

    private const int MaximumSymbolsPerConnection = 512;

    protected string MarketName { get; set; }
    protected BybitRestApiClient ApiClient { get; set; }
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
    /// <param name="algorithm">the algorithm instance is required to retrieve account type</param>
    /// <param name="aggregator">the aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    public BybitBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job)
        : this(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, algorithm, aggregator, job, Market.Bybit)
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
    /// <param name="marketName">Actual market name</param>
    public BybitBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job, string marketName)
        : base(marketName)
    {
        Initialize(
            webSocketBaseUrl,
            restApiUrl,
            apiKey,
            apiSecret,
            algorithm,
            aggregator,
            job,
            marketName
        );
    }

        


    #region Brokerage

    /// <summary>
    /// Gets all open orders on the account.
    /// NOTE: The order objects returned do not have QC order IDs.
    /// </summary>
    /// <returns>The open orders returned from IB</returns>
    public override List<Order> GetOpenOrders()
    {
        var orders = ApiClient.GetOpenOrders(Category);

        var mapped = orders.Select(item =>
        {
            var symbol = _symbolMapper.GetLeanSymbol(item.Symbol, SecurityType.CryptoFuture, Market.Bybit);
            var price = item.Price!.Value;


            Order order;
            if (item.StopOrderType != null)
            {
                if (item.StopOrderType == StopOrderType.TrailingStop)
                {
                    throw new NotImplementedException();
                }

                order = item.OrderType == OrderType.Limit
                    ? new StopLimitOrder(symbol, item.Quantity, price, item.Price!.Value, item.CreateTime)
                    : new StopMarketOrder(symbol, item.Quantity, price, item.CreateTime);
            }
            else
            {
                order = item.OrderType == OrderType.Limit
                    ? new LimitOrder(symbol, item.Quantity, price, item.CreateTime)
                    : new MarketOrder(symbol, item.Quantity, item.CreateTime);
            }

            order.BrokerId.Add(item.OrderId);
            //todo order.Status=
            // todo order.Id = item.ClientOrderId
            return order;
        });
        return mapped.ToList();
    }

    /// <summary>
    /// Gets all holdings for the account
    /// </summary>
    /// <returns>The current holdings from the account</returns>
    public override List<Holding> GetAccountHoldings()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the current cash balance for each currency held in the brokerage account
    /// </summary>
    /// <returns>The current cash balance for each currency available for trading</returns>
    public override List<CashAmount> GetCashBalance()
    {
        throw new NotImplementedException();

    }

    /// <summary>
    /// Places a new order and assigns a new broker ID to the order
    /// </summary>
    /// <param name="order">The order to be placed</param>
    /// <returns>True if the request for a new order has been placed, false otherwise</returns>
    public override bool PlaceOrder(Order order)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates the order with the same id
    /// </summary>
    /// <param name="order">The new order information</param>
    /// <returns>True if the request was made for the order to be updated, false otherwise</returns>
    public override bool UpdateOrder(Order order)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Cancels the order with the specified ID
    /// </summary>
    /// <param name="order">The order to cancel</param>
    /// <returns>True if the request was made for the order to be canceled, false otherwise</returns>
    public override bool CancelOrder(Order order)
    {
        throw new NotImplementedException();
    }

    protected override void OnMessage(object sender, WebSocketMessage e)
    {
        _messageHandler.HandleNewMessage(e);
    }

    /// <summary>
    /// Connects the client to the broker's remote servers
    /// </summary>
    public override void Connect()
    {
        if (IsConnected)
            return;

        // cannot reach this code if rest api client is not created
        // WebSocket is  responsible for Binance UserData stream only
        // as a result we don't need to connect user data stream if BinanceBrokerage is used as DQH only
        // or until Algorithm is actually initialized
           
        //todo reconnect
        if(WebSocket == null) return;
        WebSocket.Initialize(_webSocketBaseUrl);
        ConnectSync();
    }

    /// <summary>
    /// Disconnects the client from the broker's remote servers
    /// </summary>
    public override void Disconnect()
    {
        if(WebSocket?.IsOpen != true) return;
            
        _keepAliveTimer.Stop();
        WebSocket.Close();
    }

    #endregion
        
        
    protected void Initialize(string wssUrl, string restApiUrl, string apiKey, string apiSecret,
        IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job, string marketName)
    {
        if (IsInitialized)
        {
            return;
        }

        //todo why here?
        base.Initialize(wssUrl, new WebSocketClientWrapper(), null, apiKey, apiSecret);

        _job = job;
        _algorithm = algorithm;
        _aggregator = aggregator;
        _webSocketBaseUrl = wssUrl;
        _messageHandler = new BrokerageConcurrentMessageHandler<WebSocketMessage>(OnUserMessage);
        _symbolMapper = new(marketName);
        MarketName = marketName;

        var maximumWebSocketConnections = Config.GetInt("bybit-maximum-websocket-connections", 500); //todo  defailt
        //var maximumWebSocketConnections = Config.GetInt("bybit-maximum-websocket-connections");
        var symbolWeights = new Dictionary<Symbol, int>();//maximumWebSocketConnections > 0 ? FetchSymbolWeights(restApiUrl) : null;

            
        var subscriptionManager = new BrokerageMultiWebSocketSubscriptionManager(
            wssUrl,
            MaximumSymbolsPerConnection,
            512,//maximumWebSocketConnections,
            new Dictionary<Symbol, int>(),//symbolWeights,
            () => new BybitWebSocketWrapper(null),
            Subscribe,
            Unsubscribe,
            OnDataMessage,
            new TimeSpan(23, 45, 0));


        SubscriptionManager = subscriptionManager;

        // can be null, if BinanceBrokerage is used as DataQueueHandler only

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
            //todo https://public.bybit.com/
            var res = new BybitArchiveDownloader().Download(Category, brokerageSymbol, request.StartTimeUtc,
                request.EndTimeUtc); //todo end
            foreach (var tick in res)
            {
                yield return new Tick(tick.Time, request.Symbol, tick.TickDirection.ToStringInvariant(),
                    Exchange.Bybit, tick.Size, tick.price)
                {
                };

                //todo what about direction?
            }

            yield break;
        }

        var client = ApiClient ?? new BybitRestApiClient(_symbolMapper, null, null, null,
            Config.Get("bybit-api-url",
                "https://api.bybit.com")); //todo get url from somewhere else, I don't like calling the config from eveywhere.. get rid of the lazy thingy .. what is binance doning?

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

        /* Todo
             
            return (_algorithm == null || _algorithm.BrokerageModel.AccountType == AccountType.Cash)
                 ? new BinanceSpotRestApiClient(symbolMapper, securityProvider, apiKey, apiSecret, restApiUrl)
                 : new BinanceCrossMarginRestApiClient(symbolMapper, securityProvider, apiKey, apiSecret,
                     restApiUrl);
             */
        return new BybitRestApiClient(symbolMapper, securityProvider, apiSecret, apiKey, restApiUrl);
    }

    public override void Dispose()
    {
        _keepAliveTimer.DisposeSafely();
        _webSocketRateLimiter.DisposeSafely();
        SubscriptionManager.DisposeSafely();

        base.Dispose();
    }
}