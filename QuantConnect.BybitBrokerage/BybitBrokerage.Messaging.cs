using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.BybitBrokerage.Models.Messages;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Securities;
using QuantConnect.Util;
using OrderStatus = QuantConnect.BybitBrokerage.Models.Enums.OrderStatus;

namespace QuantConnect.BybitBrokerage;

public partial class BybitBrokerage
{
    private class StreamAuthenticatedEventArgs : EventArgs
    {
        public string Message { get; init; }
        public bool IsAuthenticated { get; init; }

        public StreamAuthenticatedEventArgs(bool isAuthenticated, string message)
        {
            IsAuthenticated = isAuthenticated;
            Message = message;
        }
    }

    private static readonly JsonSerializerSettings Settings = new()
    {
        Converters = new List<JsonConverter>() { new ByBitKlineJsonConverter(), new BybitDecimalStringConverter() },
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
    };

    private static JsonSerializer JsonSerializer => JsonSerializer.CreateDefault(Settings);

    private event EventHandler<StreamAuthenticatedEventArgs> Authenticated;

    private readonly object _tickLocker = new();
    private readonly ConcurrentDictionary<Symbol, DefaultOrderBook> _orderBooks = new();


    private void OnUserMessage(WebSocketMessage webSocketMessage)
    {
        var e = (WebSocketClientWrapper.TextMessage)webSocketMessage.Data;
        try
        {
            if (Log.DebuggingEnabled)
            {
                Log.Debug($"{nameof(BybitBrokerage)}.{nameof(OnUserMessage)}(): {e.Message}");
            }

            var jObj = JObject.Parse(e.Message);
            if (jObj.TryGetValue("op", out _))
            {
                HandleOperationMessage(jObj);
                return;
            }

            var topic = jObj.Value<string>("topic");
            switch (topic)
            {
                case "order":
                    HandleOrderUpdate(jObj);
                    break;
                case "execution":
                    HandleOrderExecution(jObj);
                    break;
            }
        }
        catch (Exception exception)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                $"Parsing wss message failed. Data: {e.Message} Exception: {exception}"));
            throw;
        }
    }

    private void HandleOrderExecution(JObject jObject)
    {
        var tradeUpdates = jObject.ToObject<BybitDataMessage<BybitTradeUpdate[]>>(JsonSerializer).Data;
        foreach (var tradeUpdate in tradeUpdates)
        {
            var leanOrder = OrderProvider.GetOrdersByBrokerageId(tradeUpdate.OrderId).FirstOrDefault();
            if (leanOrder == null) continue;

            if (tradeUpdate.ExecutionType is not (ExecutionType.Trade))
            {
                Log.Trace(jObject.ToString());
                continue; //todo verify
            }

            var symbol = tradeUpdate.Symbol;
            var leanSymbol = _symbolMapper.GetLeanSymbol(symbol, GetSupportedSecurityType(), MarketName);
            var status = tradeUpdate.QuantityRemaining.GetValueOrDefault(0) == 0
                ? QuantConnect.Orders.OrderStatus.Filled
                : QuantConnect.Orders.OrderStatus.PartiallyFilled;

            var fee = OrderFee.Zero;
            if (tradeUpdate.ExecutionFee != 0)
            {
                var currency = Category switch
                {
                    BybitProductCategory.Linear => "USDT",
                    BybitProductCategory.Inverse => GetBaseCurrency(symbol),
                    BybitProductCategory.Spot => GetSpotFeeCurrency(leanSymbol, tradeUpdate),
                    _ => throw new NotSupportedException($"category {Category.ToString()} not implemented")
                };
                fee = new OrderFee(new CashAmount(tradeUpdate.ExecutionFee, currency));
            }

            var orderEvent = new OrderEvent(
                leanOrder.Id,
                leanSymbol,
                tradeUpdate.ExecutionTime, status,
                tradeUpdate.Side == OrderSide.Buy ? OrderDirection.Buy : OrderDirection.Sell,
                tradeUpdate.ExecutionPrice,
                tradeUpdate.ExecutionQuantity,
                fee);

            Log.Trace(
                $"Orderstatus changed {leanOrder.Status.ToStringInvariant()} => {status.ToStringInvariant()} from {tradeUpdate.ExecutionType?.ToStringInvariant()}");
            TestFix(leanOrder.Id, status);
            OnOrderEvent(orderEvent);
        }

        static string GetSpotFeeCurrency(Symbol symbol, BybitTradeUpdate tradeUpdate)
        {
            CurrencyPairUtil.DecomposeCurrencyPair(symbol, out var @base, out var quote);
            if (tradeUpdate.FeeRate > 0 || tradeUpdate.IsMaker)
            {
                return tradeUpdate.Side == OrderSide.Buy ? @base : quote;
            }


            return tradeUpdate.Side == OrderSide.Buy ? quote : @base;
        }

        static string GetBaseCurrency(string pair)
        {
            CurrencyPairUtil.DecomposeCurrencyPair(pair, out var baseCurrency, out _);
            return baseCurrency;
        }
    }

    private void HandleOrderUpdate(JObject jObject)
    {
        var orders = jObject.ToObject<BybitDataMessage<BybitOrder[]>>(JsonSerializer).Data;
        foreach (var order in orders)
        {
            //We're not interested in order executions here as HandleOrderExecution is taking care of this
            if (order.Status is OrderStatus.Filled or OrderStatus.PartiallyFilled) continue;

            //This should imo only be one order but the test OrderProvider is cloning them
            var leanOrder = OrderProvider.GetOrdersByBrokerageId(order.OrderId).FirstOrDefault();
            if (leanOrder == null) continue;

            var newStatus = ConvertOrderStatus(order.Status);
            if (newStatus == leanOrder.Status) continue;


            var orderEvent = new OrderEvent(leanOrder, order.UpdateTime, OrderFee.Zero) { Status = newStatus };
            TestFix(leanOrder.Id, newStatus);
            OnOrderEvent(orderEvent);
        }
    }

    private void HandleOrderBookUpdate(JObject jObject)
    {
        var orderBookUpdate = jObject.ToObject<BybitDataMessage<BybitOrderBookUpdate>>(JsonSerializer);
        var orderBookData = orderBookUpdate.Data;

        if (orderBookUpdate.Type == BybitMessageType.Snapshot || orderBookUpdate.Data.UpdateId == 1)
        {
            HandleOrderBookSnapshot(orderBookData);
        }
        //delta
        else
        {
            HandleOrderBookDelta(orderBookData);
        }
    }

    private void HandleOrderBookSnapshot(BybitOrderBookUpdate orderBookUpdate)
    {
        var symbol = _symbolMapper.GetLeanSymbol(orderBookUpdate.Symbol, GetSupportedSecurityType(), MarketName);

        if (!_orderBooks.TryGetValue(symbol, out var orderBook))
        {
            orderBook = new DefaultOrderBook(symbol);
            _orderBooks[symbol] = orderBook;
        }
        else
        {
            orderBook.BestBidAskUpdated -= OnBestBidAskUpdated;
            orderBook.Clear();
        }

        foreach (var row in orderBookUpdate.Bids)
        {
            orderBook.UpdateBidRow(row.Price, row.Size);
        }

        foreach (var row in orderBookUpdate.Asks)
        {
            orderBook.UpdateAskRow(row.Price, row.Size);
        }

        orderBook.BestBidAskUpdated += OnBestBidAskUpdated;
        EmitQuoteTick(symbol, orderBook.BestBidPrice, orderBook.BestBidSize, orderBook.BestAskPrice,
            orderBook.BestAskSize);
    }

    private void HandleOrderBookDelta(BybitOrderBookUpdate orderBookUpdate)
    {
        var symbol = _symbolMapper.GetLeanSymbol(orderBookUpdate.Symbol, GetSupportedSecurityType(), MarketName);

        if (!_orderBooks.TryGetValue(symbol, out var orderBook))
        {
            Log.Error($"Attempting to update a non existent order book for {symbol}");
            return;
        }

        foreach (var row in orderBookUpdate.Bids)
        {
            if (row.Size == 0)
            {
                orderBook.RemoveBidRow(row.Price);
            }
            else
            {
                orderBook.UpdateBidRow(row.Price, row.Size);
            }
        }

        foreach (var row in orderBookUpdate.Asks)
        {
            if (row.Size == 0)
            {
                orderBook.RemoveAskRow(row.Price);
            }
            else
            {
                orderBook.UpdateAskRow(row.Price, row.Size);
            }
        }
    }

    private void OnBestBidAskUpdated(object sender, BestBidAskUpdatedEventArgs e)
    {
        EmitQuoteTick(e.Symbol, e.BestBidPrice, e.BestBidSize, e.BestAskPrice, e.BestAskSize);
    }

    //Todo: This needs to be removed, but for now it fixes the issues I was facing with the tests expecting the status of the original order object being
    //      updated. While the OrderProvider being used in the test returns copies of each order.
    private void TestFix(int orderId, Orders.OrderStatus status)
    {
        OrderProvider.GetOrders(x =>
        {
            if (x.Id == orderId)
            {
                x.Status = status;
                return true;
            }

            return false;
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        }).ToArray();
    }

    private void OnDataMessage(WebSocketMessage webSocketMessage)
    {
        var data = (WebSocketClientWrapper.TextMessage)webSocketMessage.Data;
        try
        {
            if (Log.DebuggingEnabled)
            {
                Log.Debug($"{nameof(BybitBrokerage)}.{nameof(OnDataMessage)}(): {data.Message}");
            }

            var obj = JObject.Parse(data.Message);
            if (obj.TryGetValue("op", out _))
            {
                HandleOperationMessage(obj);
            }
            else if (obj.TryGetValue("topic", out var topic))
            {
                var topicStr = topic.Value<string>();
                if (topicStr.StartsWith("publicTrade"))
                {
                    HandleTradeMessage(obj);
                }
                else if (topicStr.StartsWith("orderbook"))
                {
                    HandleOrderBookUpdate(obj);
                }
            }
        }
        catch (Exception e)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                $"Parsing wss message failed. Data: {e.Message} Exception: {e}"));
        }
    }

    private void HandleTradeMessage(JToken message)
    {
        var trades = message.ToObject<BybitDataMessage<BybitTickUpdate[]>>();
        foreach (var trade in trades.Data)
        {
            //Todo validate, we were talking about this in the meeting, negative value should be possible here as each trade always has a direction
            var tradeValue = trade.Side == OrderSide.Buy ? trade.Value : trade.Value * -1;
            EmitTradeTick(_symbolMapper.GetLeanSymbol(trade.Symbol, GetSupportedSecurityType(), MarketName), trade.Time,
                trade.Price, tradeValue);
        }
    }

    private void HandleOperationMessage(JToken message)
    {
        var dataMessage = message.ToObject<BybitOperationResponseMessage>();
        if (dataMessage.Operation == "subscribe" && !dataMessage.Success)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                $"Subscription failed: {dataMessage.ReturnMessage}"));
        }

        if (dataMessage.Operation == "auth")
        {
            Authenticated?.Invoke(this,
                new StreamAuthenticatedEventArgs(dataMessage.Success, dataMessage.ReturnMessage));
            if (!dataMessage.Success)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                    $"Unable to authenticate private stream: {dataMessage.ReturnMessage}"));
            }
            else
            {
                Log.Trace("BybitBrokerage.HandleOpMessage() Successfully authenticated private stream");
            }
        }
    }

    private void EmitTradeTick(Symbol symbol, DateTime time, decimal price, decimal quantity)
    {
        var tick = new Tick
        {
            Symbol = symbol,
            Value = price,
            Quantity = Math.Abs(quantity),
            Time = time,
            TickType = TickType.Trade
        };

        lock (_tickLocker)
        {
            _aggregator.Update(tick);
        }
    }

    private void EmitQuoteTick(Symbol symbol, decimal bidPrice, decimal bidSize, decimal askPrice, decimal askSize)
    {
        var tick = new Tick
        {
            AskPrice = askPrice,
            BidPrice = bidPrice,
            Time = DateTime.UtcNow,
            Symbol = symbol,
            TickType = TickType.Quote,
            AskSize = askSize,
            BidSize = bidSize,
        };
        tick.SetValue();

        lock (_tickLocker)
        {
            _aggregator.Update(tick);
        }
    }


    /// <summary>
    /// Subscribes to the requested symbol (using an individual streaming channel)
    /// </summary>
    /// <param name="webSocket">The websocket instance</param>
    /// <param name="symbol">The symbol to subscribe</param>
    private bool Subscribe(IWebSocket webSocket, Symbol symbol)
    {
        var depthString = _orderBookDepth.ToStringInvariant();
        if (!IsOrderBookDepthSupported(Category, _orderBookDepth))
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                $"Configured order book depth of '{depthString}' is not supported by {Category.ToStringInvariant()}"));
            return false;
        }

        var brokerageSymbol = _symbolMapper.GetBrokerageSymbol(symbol);
        Send(webSocket,
            new
            {
                op = "subscribe",
                args = new[]
                {
                    $"publicTrade.{brokerageSymbol}",
                    $"orderbook.{depthString}.{brokerageSymbol}"
                    //$"tickers.{s}" //Push frequency: Derivatives & Options - 100ms, Spot - real-time
                }
            }
        );
        return true;
    }


    /// <summary>
    /// Ends current subscription
    /// </summary>
    /// <param name="webSocket">The websocket instance</param>
    /// <param name="symbol">The symbol to unsubscribe</param>
    private bool Unsubscribe(IWebSocket webSocket, Symbol symbol)
    {
        var brokerageSymbol = _symbolMapper.GetBrokerageSymbol(symbol);
        var depthString = _orderBookDepth.ToStringInvariant();
        Send(webSocket,
            new
            {
                op = "unsubscribe",
                args = new[]
                {
                    $"publicTrade.{brokerageSymbol}",
                    $"orderbook.{depthString}.{brokerageSymbol}"
                    //$"tickers.{leanSymbol}"
                }
            }
        );

        return true;
    }

    private void Connect(BybitApi api)
    {
        if (WebSocket == null) return;


        WebSocket.Initialize(_privateWebSocketUrl);

        // When connect is called from the api client factory the ApiClient instance property is not set yet
        // to get rid of the deferred reference the OnOpen handler is replaced with the instance handler
        api ??= ApiClient;

        //todo maybe there is a better place to validate this
        if (!IsAccountMarginStatusValid(api, out var message))
        {
            OnMessage(message);
            return;
        }

        ConnectSync();

        // The initial authentication is done in sync to interrupt the brokerage initialization as this is a hard error
        if (!AuthenticatePrivateWSAndWait(api))
        {
            throw new Exception("Unable to connect to client");
        }

        WebSocket.Open += OnPrivateWSConnected;
    }

    private void OnPrivateWSConnected(object sender, EventArgs eventArgs)
    {
        AuthenticatePrivateWS(ApiClient, TimeSpan.FromSeconds(30));
    }

    private void OnPrivateWSAuthenticated(object _, StreamAuthenticatedEventArgs args)
    {
        if (args.IsAuthenticated)
        {
            Send(WebSocket, new { op = "subscribe", args = new[] { "order", "execution" } });
        }
    }

    private bool AuthenticatePrivateWSAndWait(BybitApi api)
    {
        var resetEvent = new ManualResetEvent(false);
        var authenticated = false;

        void OnAuthenticated(object _, StreamAuthenticatedEventArgs args)
        {
            resetEvent.Set();
            authenticated = args.IsAuthenticated;
        }

        Authenticated += OnAuthenticated;
        var authValidFor = TimeSpan.FromSeconds(30);

        AuthenticatePrivateWS(api, authValidFor);
        resetEvent.WaitOne(authValidFor);

        Authenticated -= OnAuthenticated;
        return authenticated;
    }

    private void AuthenticatePrivateWS(BybitApi api, TimeSpan authValidFor)
    {
        Send(WebSocket, api.AuthenticateWebSocket(authValidFor));
    }

    private static bool IsOrderBookDepthSupported(BybitProductCategory category, int depth)
    {
        /*
           Order book push frequencies
           
           Linear & inverse:
           Level 1 data, push frequency: 10ms
           Level 50 data, push frequency: 20ms
           Level 200 data, push frequency: 100ms
           Level 500 data, push frequency: 100ms

           Spot:
           Level 1 data, push frequency: 10ms
           Level 50 data, push frequency: 20ms

           Option:
           Level 25 data, push frequency: 20ms
           Level 100 data, push frequency: 100ms
        */
        switch (category)
        {
            case BybitProductCategory.Spot:
                return Array.IndexOf(new[] { 1, 50 }, depth) >= 0;
            case BybitProductCategory.Linear:
            case BybitProductCategory.Inverse:
                return Array.IndexOf(new[] { 1, 50, 200, 500 }, depth) >= 0;
            case BybitProductCategory.Option:
                return Array.IndexOf(new[] { 25, 100 }, depth) >= 0;
            default:
                return false;
        }
    }

    private static bool IsAccountMarginStatusValid(BybitApi api, out BrokerageMessageEvent message)
    {
        var accountInfo = api.Account.GetAccountInfo();
        if (accountInfo.UnifiedMarginStatus is not (AccountUnifiedMarginStatus.UnifiedTrade
            or AccountUnifiedMarginStatus.UTAPro))
        {
            message = new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                "Only unified margin trade accounts are supported");
            return false;
        }

        message = null;
        return true;
    }

    private static void Send(IWebSocket webSocket, object obj)
    {
        var json = JsonConvert.SerializeObject(obj, Settings);
        if (Log.DebuggingEnabled)
        {
            Log.Debug("BybitBrokerage.Send(): " + json);
        }

        webSocket.Send(json);
    }
}