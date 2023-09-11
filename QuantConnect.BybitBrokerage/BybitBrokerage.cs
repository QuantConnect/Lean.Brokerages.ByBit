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
using System.Collections.Generic;
using System.Linq;
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

/// <summary>
/// Bybit brokerage implementation
/// todo: margin, oi funding
/// </summary>
[BrokerageFactory(typeof(BybitBrokerageFactory))]
public partial class BybitBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
{
    private int _orderBookDepth;

    private IAlgorithm _algorithm;
    private SymbolPropertiesDatabaseSymbolMapper _symbolMapper;
    private LiveNodePacket _job;
    private string _privateWebSocketUrl;
    private Lazy<BybitApi> _apiClientLazy;

    private BrokerageConcurrentMessageHandler<WebSocketMessage> _messageHandler;

    /// <summary>
    /// Brokerage market name
    /// </summary>
    protected string MarketName { get; set; }

    /// <summary>
    /// Order provider 
    /// </summary>
    protected IOrderProvider OrderProvider { get; private set; }

    /// <summary>
    /// Api client instance
    /// </summary>
    protected BybitApi ApiClient => _apiClientLazy?.Value;

    /// <summary>
    /// Account category
    /// </summary>
    protected virtual BybitProductCategory Category => BybitProductCategory.Spot;

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
    /// <param name="algorithm">the algorithm instance is required to retrieve account type</param>
    /// <param name="aggregator">the aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    /// <param name="orderBookDepth">The requested order book depth</param>
    /// <param name="vipLevel">Bybit VIP level</param>
    public BybitBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IDataAggregator aggregator, LiveNodePacket job,
        int orderBookDepth,
        BybitVIPLevel vipLevel = BybitVIPLevel.VIP0)
        : this(apiKey, apiSecret, restApiUrl, webSocketBaseUrl, algorithm, algorithm?.Portfolio?.Transactions,
            algorithm?.Portfolio, aggregator, job, Market.Bybit, orderBookDepth, vipLevel)
    {
    }

    /// <summary>
    /// Constructor for brokerage
    /// </summary>
    /// <param name="webSocketBaseUrl">The web socket base url</param>
    /// <param name="restApiUrl">The rest api url</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="algorithm">The algorithm instance is required to retrieve account type</param>
    /// <param name="orderProvider">The order provider is required to retrieve orders</param>
    /// <param name="securityProvider">The security provider is required</param>
    /// <param name="aggregator">The aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    /// <param name="marketName">Market name</param>
    /// <param name="orderBookDepth">The requested order book depth</param>
    /// <param name="vipLevel">Bybit VIP level</param>
    public BybitBrokerage(string apiKey, string apiSecret, string restApiUrl, string webSocketBaseUrl,
        IAlgorithm algorithm, IOrderProvider orderProvider, ISecurityProvider securityProvider,
        IDataAggregator aggregator, LiveNodePacket job, string marketName, int orderBookDepth,
        BybitVIPLevel vipLevel = BybitVIPLevel.VIP0)
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
            marketName,
            orderBookDepth,
            vipLevel
        );
    }

    /// <summary>
    /// Gets the history for the requested security
    /// </summary>
    /// <param name="request">The historical data request</param>
    /// <returns>An enumerable of bars or ticks covering the span specified in the request</returns>
    public override IEnumerable<BaseData> GetHistory(HistoryRequest request)
    {
        if (!_symbolMapper.IsKnownLeanSymbol(request.Symbol))
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidSymbol",
                $"Unknown symbol: {request.Symbol.Value}, no history returned"));
            return Array.Empty<BaseData>();
        }

        if (request.Resolution == Resolution.Second)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidResolution",
                $"{request.Resolution} resolution is not supported, no history returned"));
            return Array.Empty<BaseData>();
        }

        if (request.TickType is not (TickType.OpenInterest or TickType.Trade))
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidTickType",
                $"{request.TickType} tick type not supported, no history returned"));
            return Array.Empty<BaseData>();
        }

       
        if (request.Symbol.SecurityType != GetSupportedSecurityType())
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, "InvalidSecurityType",
                $"{request.Symbol.SecurityType} security type not supported, no history returned"));
            return Array.Empty<BaseData>();
        }

        var brokerageSymbol = _symbolMapper.GetBrokerageSymbol(request.Symbol);

        if (request.TickType == TickType.OpenInterest)
        {
            return GetOpenInterestHistory(brokerageSymbol, request);
        }
        
        if (request.Resolution == Resolution.Tick)
        {
            return GetTickHistory(brokerageSymbol, request);
        }

        return GetBarHistory(brokerageSymbol, request);
    }

    /// <summary>
    /// Initialize the instance of this class
    /// </summary>
    /// <param name="baseWssUrl">The web socket base url</param>
    /// <param name="restApiUrl">The rest api url</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="algorithm">The algorithm instance is required to retrieve account type</param>
    /// <param name="orderProvider">The order provider is required to retrieve orders</param>
    /// <param name="securityProvider">The security provider is required</param>
    /// <param name="aggregator">The aggregator for consolidating ticks</param>
    /// <param name="job">The live job packet</param>
    /// <param name="marketName">Market name</param>
    /// <param name="orderBookDepth">The requested order book depth</param>
    /// <param name="vipLevel">Bybit VIP level</param>
    private void Initialize(string baseWssUrl, string restApiUrl, string apiKey, string apiSecret,
        IAlgorithm algorithm, IOrderProvider orderProvider, ISecurityProvider securityProvider,
        IDataAggregator aggregator, LiveNodePacket job,
        string marketName, int orderBookDepth, BybitVIPLevel vipLevel)
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
        _orderBookDepth = orderBookDepth;
        _messageHandler = new BrokerageConcurrentMessageHandler<WebSocketMessage>(OnUserMessage);
        _symbolMapper = new(marketName);
        OrderProvider = orderProvider;
        MarketName = marketName;

        //todo validate
        var weights = new Dictionary<Symbol, int>();
        using (var tempClient = GetApiClient(_symbolMapper, securityProvider, restApiUrl, apiKey, apiSecret, vipLevel))
        {
            foreach (var ticker in tempClient.Market.GetTickers(Category))
            {
                Symbol leanSymbol;
                try
                {
                    leanSymbol = _symbolMapper.GetLeanSymbol(ticker.Symbol, GetSupportedSecurityType(), MarketName);
                }
                catch (Exception)
                {
                    //The api returns some currently unsupported symbols we can ignore these right now
                    continue;
                }

                var weight = (ticker.Turnover24Hours > int.MaxValue)
                    ? int.MaxValue
                    : decimal.ToInt32(ticker.Turnover24Hours ?? 0);
                
                weights.Add(leanSymbol,weight);
            }
        }
        
        var subscriptionManager = new BrokerageMultiWebSocketSubscriptionManager(publicWssUrl,
            16,
            128,
            weights,
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
                // Api credentials are required for the private stream

                if (string.IsNullOrEmpty(apiSecret) || string.IsNullOrEmpty(apiSecret))
                {
                    OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1, "Api credentials missing"));
                    throw new Exception("Api credentials missing");
                }
                var client = GetApiClient(_symbolMapper, securityProvider, restApiUrl, apiKey, apiSecret, vipLevel);

                //Lazy connection to the private stream as it's not required when the brokerage is only used as DataQueueHandler

                Authenticated += OnPrivateWSAuthenticated;

                Connect(client);
                return client;
            });
        }

        //todo ValidateSubscription(); 
    }

    /// <summary>
    /// Checks if this brokerage supports the specified symbol
    /// </summary>
    /// <param name="symbol">The symbol</param>
    /// <returns>returns true if brokerage supports the specified symbol; otherwise false</returns>
    protected virtual bool CanSubscribe(Symbol symbol)
    {
        return !symbol.Value.Contains("UNIVERSE") &&
               symbol.SecurityType == GetSupportedSecurityType() &&
               symbol.ID.Market == MarketName;
    }

    /// <summary>
    /// Gets the supported security type by the brokerage
    /// </summary>
    protected virtual SecurityType GetSupportedSecurityType()
    {
        return SecurityType.Crypto;
    }


    /// <summary>
    /// Not used
    /// </summary>
    /// <param name="symbols">The symbols to be added keyed by SecurityType</param>
    protected override bool Subscribe(IEnumerable<Symbol> symbols)
    {
        // NOP
        return true;
    }

    /// <summary>
    /// Gets the appropriate API client to use
    /// </summary>
    protected virtual BybitApi GetApiClient(ISymbolMapper symbolMapper, ISecurityProvider securityProvider,
        string restApiUrl, string apiKey, string apiSecret, BybitVIPLevel vipLevel)
    {
        return new BybitApi(symbolMapper, securityProvider, apiKey, apiSecret, restApiUrl, vipLevel);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        SubscriptionManager.DisposeSafely();
        if (_apiClientLazy?.IsValueCreated == true)
        {
            ApiClient.DisposeSafely();
        }

        base.Dispose();
    }

    private IEnumerable<Tick> GetTickHistory(string brokerageSymbol, HistoryRequest request)
    {
        var res = new BybitHistoryApi()
            .Download(Category, brokerageSymbol, request.StartTimeUtc, request.EndTimeUtc);

        foreach (var tick in res)
        {
            yield return
                new Tick(tick.Time, request.Symbol, string.Empty, MarketName,
                    tick.Value * (tick.Side == OrderSide.Buy ? 1m : -1m), tick.Price);
        }
    }

    private IEnumerable<TradeBar> GetBarHistory(string brokerageSymbol, HistoryRequest request)
    {
        var client = ApiClient ?? GetApiClient(_symbolMapper, null,
            Config.Get("bybit-api-url", "https://api.bybit.com"), null, null, BybitVIPLevel.VIP0);

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

    private IEnumerable<OpenInterest> GetOpenInterestHistory(string brokerageSymbol, HistoryRequest request)
    {
        var client = ApiClient ?? GetApiClient(_symbolMapper, null,
            Config.Get("bybit-api-url", "https://api.bybit.com"), null, null, BybitVIPLevel.VIP0);
        var oiHistory = client.Market
            .GetOpenInterest(Category, brokerageSymbol, request.Resolution, request.StartTimeUtc, request.EndTimeUtc);
        
        foreach (var oi in oiHistory)
        {
            yield return new OpenInterest(oi.Time, request.Symbol, oi.OpenInterest);
        }
    }

    private static OrderStatus ConvertOrderStatus(Models.Enums.OrderStatus orderStatus)
    {
        switch (orderStatus)
        {
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