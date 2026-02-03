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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages.Bybit.Api;
using QuantConnect.Brokerages.Bybit.Converters;
using QuantConnect.Brokerages.Bybit.Models;
using QuantConnect.Brokerages.Bybit.Models.Enums;
using QuantConnect.Brokerages.Bybit.Models.Messages;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Securities;
using QuantConnect.Util;
using OrderStatus = QuantConnect.Brokerages.Bybit.Models.Enums.OrderStatus;

namespace QuantConnect.Brokerages.Bybit;

public partial class BybitBrokerage
{
    private readonly ConcurrentDictionary<int, decimal> _expectedFillQuantity = new();
    private readonly ConcurrentDictionary<int, decimal> _cumulativeFillQuantity = new();

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


    /// <summary>
    /// Processes WSS messages from the private user data streams
    /// </summary>
    /// <param name="webSocketMessage">The message to process</param>
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

    private void HandleOrderExecution(JToken message)
    {
        var tradeUpdates = message.ToObject<BybitDataMessage<BybitTradeUpdate[]>>(JsonSerializer).Data;
        foreach (var tradeUpdate in tradeUpdates)
        {
            var leanOrder = OrderProvider.GetOrdersByBrokerageId(tradeUpdate.OrderId).FirstOrDefault();
            if (leanOrder == null) continue;

            // We are only interested in actual order executions, other types like liquidations are not needed. Todo what does lean need in case of liquidations?
            if (tradeUpdate.ExecutionType is not ExecutionType.Trade)
            {
                continue;
            }

            var symbol = tradeUpdate.Symbol;
            var leanSymbol = _symbolMapper.GetLeanSymbol(symbol, GetSecurityType(tradeUpdate.Category), MarketName);
            var filledQuantity = Math.Abs(tradeUpdate.ExecutionQuantity);

            var expectedQuantity = leanOrder.AbsoluteQuantity;
            if (_expectedFillQuantity.TryGetValue(leanOrder.Id, out var cumBrokerageExecQty))
            {
                expectedQuantity = cumBrokerageExecQty;
            }

            _cumulativeFillQuantity.TryGetValue(leanOrder.Id, out var accumulatedFilledQuantity);

            var status = Orders.OrderStatus.PartiallyFilled;
            if (accumulatedFilledQuantity + filledQuantity == expectedQuantity)
            {
                status = Orders.OrderStatus.Filled;
                _expectedFillQuantity.TryRemove(leanOrder.Id, out _);
                _cumulativeFillQuantity.TryRemove(leanOrder.Id, out _);
            }
            else
            {
                _cumulativeFillQuantity[leanOrder.Id] = accumulatedFilledQuantity + filledQuantity;
            }

            var fee = OrderFee.Zero;
            if (tradeUpdate.ExecutionFee != 0)
            {
                var currency = tradeUpdate.Category switch
                {
                    BybitProductCategory.Linear => "USDT",
                    BybitProductCategory.Inverse => GetBaseCurrency(symbol),
                    BybitProductCategory.Spot => GetSpotFeeCurrency(leanSymbol, tradeUpdate),
                    _ => throw new NotSupportedException($"category {tradeUpdate.Category} not implemented")
                };
                fee = new OrderFee(new CashAmount(tradeUpdate.ExecutionFee, currency));
            }

            var orderEvent = new OrderEvent(
                leanOrder.Id,
                leanSymbol,
                tradeUpdate.ExecutionTime, status,
                tradeUpdate.Side == OrderSide.Buy ? OrderDirection.Buy : OrderDirection.Sell,
                tradeUpdate.ExecutionPrice,
                filledQuantity * Math.Sign(leanOrder.Quantity),
                fee);

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

    /// <summary>
    /// This method is emitting events when orders are cancelled
    /// </summary>
    private void HandleOrderUpdate(JToken message)
    {
        var orders = message.ToObject<BybitDataMessage<BybitOrder[]>>(JsonSerializer).Data;
        foreach (var order in orders)
        {
            //We're not interested in order executions here as HandleOrderExecution is taking care of this
            if (order is { Status: OrderStatus.PartiallyFilled }) continue;

            var leanOrder = OrderProvider.GetOrdersByBrokerageId(order.OrderId).FirstOrDefault();
            if (leanOrder == null) continue;

            var newStatus = ConvertOrderStatus(order.Status);
            if (newStatus == leanOrder.Status) continue;

            if (order is { Status: OrderStatus.Filled })
            {
                if (order is { QuantityFilled: null })
                {
                    continue;
                }

                if (!_cumulativeFillQuantity.TryGetValue(leanOrder.Id, out var cumulativeFill)
                    || cumulativeFill < order.QuantityFilled.Value)
                {
                    // OrderUpdate event arrived too early, we expect more Executions
                    _expectedFillQuantity.TryAdd(leanOrder.Id, order.QuantityFilled.Value);

                    continue;
                }

                // We need to issue the OrderStatus.Filled event
                // as HandleOrderExecution issued only PartiallyFilled events, and no more events are expected
                // Order is fully filled, but less than Lean expected, i.e.
                // Bybit can slightly reduce the order size if it was partially filled initialy, see json example (cumExecQty, leavesQty, leavesValue) for the Buy 0.0001 (reduced by the lot size)
                // {
                //   "topic": "order",
                //   "id": "106619370_20000_2021269409",
                //   "creationTime": 1769798300234,
                //   "data": [
                //     {
                //       "category": "spot",
                //       "symbol": "BTCUSDT"
                //       "side": "Buy",
                //       "orderStatus": "Filled",
                //       "leavesQty": "0",
                //       "leavesValue": "0.0764398",
                //       "cumExecQty": "0.000099",
                //       "cumExecValue": "8.1615402",
                //       "cumExecFee": "0.0000000891",
                //       "orderType": "Market",
                //       ... rest fields
                //     }
                //   ]
                // }
            }

            if (newStatus.IsClosed())
            {
                _expectedFillQuantity.TryRemove(leanOrder.Id, out _);
                _cumulativeFillQuantity.TryRemove(leanOrder.Id, out _);
            }

            var orderEvent = new OrderEvent(leanOrder, order.UpdateTime, OrderFee.Zero) { Status = newStatus };
            OnOrderEvent(orderEvent);
        }
    }


    /// <summary>
    /// Processes WSS messages from the public market data streams
    /// </summary>
    /// <param name="webSocketMessage">The message to process</param>
    private void OnDataMessage(WebSocketMessage webSocketMessage, BybitProductCategory category)
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

            // The topic for market data is {topic}.{symbol}
            else if (obj.TryGetValue("topic", out var topic))
            {
                var topicStr = topic.Value<string>();
                if (topicStr.StartsWith("publicTrade"))
                {
                    HandleTradeMessage(obj, category);
                }
                else if (topicStr.StartsWith("orderbook"))
                {
                    HandleOrderBookUpdate(obj, category);
                }
                else if (topicStr.StartsWith("tickers"))
                {
                    HandleTickerMessage(obj, category);
                }
            }
        }
        catch (Exception e)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                $"Parsing wss message failed. Data: {e.Message} Exception: {e}"));
        }
    }

    private void HandleTickerMessage(JToken message, BybitProductCategory category)
    {
        var tickerMessage = JsonConvert.DeserializeObject<BybitDataMessage<BybitTicker>>(message.ToString(), Settings);
        var ticker = tickerMessage.Data;

        if (!ticker.OpenInterest.HasValue) return;

        var leanSymbol = _symbolMapper.GetLeanSymbol(ticker.Symbol, GetSecurityType(category), MarketName);

        var tick = new OpenInterest(tickerMessage.Time, leanSymbol, ticker.OpenInterest.Value);
        lock (_tickLocker)
        {
            _aggregator.Update(tick);
        }
    }

    private void HandleTradeMessage(JToken message, BybitProductCategory category)
    {
        var trades = message.ToObject<BybitDataMessage<BybitTickUpdate[]>>();
        foreach (var trade in trades.Data)
        {
            // var tradeValue = trade.Side == OrderSide.Buy ? trade.Value : trade.Value * -1;
            EmitTradeTick(_symbolMapper.GetLeanSymbol(trade.Symbol, GetSecurityType(category), MarketName), trade.Time,
                trade.Price, trade.Quantity);
        }
    }

    private void HandleOrderBookUpdate(JObject jObject, BybitProductCategory category)
    {
        var orderBookUpdate = jObject.ToObject<BybitDataMessage<BybitOrderBookUpdate>>(JsonSerializer);
        var orderBookData = orderBookUpdate.Data;

        if (orderBookUpdate.Type == BybitMessageType.Snapshot || orderBookUpdate.Data.UpdateId == 1)
        {
            HandleOrderBookSnapshot(orderBookData, category);
        }
        // Delta
        else
        {
            HandleOrderBookDelta(orderBookData, category);
        }
    }

    private void HandleOrderBookSnapshot(BybitOrderBookUpdate orderBookUpdate, BybitProductCategory category)
    {
        var symbol = _symbolMapper.GetLeanSymbol(orderBookUpdate.Symbol, GetSecurityType(category), MarketName);

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
        if (orderBook.BestBidPrice == 0 && orderBook.BestAskPrice == 0)
        {
            // nothing to emit, can happen with illiquid assets
            return;
        }

        EmitQuoteTick(symbol, orderBook.BestBidPrice, orderBook.BestBidSize, orderBook.BestAskPrice,
            orderBook.BestAskSize);
    }

    private void HandleOrderBookDelta(BybitOrderBookUpdate orderBookUpdate, BybitProductCategory category)
    {
        var symbol = _symbolMapper.GetLeanSymbol(orderBookUpdate.Symbol, GetSecurityType(category), MarketName);

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
            if (!dataMessage.Success)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                    $"Unable to authenticate private stream: {dataMessage.ReturnMessage}"));
            }
            else
            {
                Log.Trace("BybitBrokerage.HandleOpMessage() Successfully authenticated private stream");
            }

            Authenticated?.Invoke(this,
                new StreamAuthenticatedEventArgs(dataMessage.Success, dataMessage.ReturnMessage));
        }
    }

    private void EmitTradeTick(Symbol symbol, DateTime time, decimal price, decimal quantity)
    {
        var tick = new Tick
        {
            Symbol = symbol,
            Value = price,
            Quantity = quantity,
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
        Send(webSocket,
            new
            {
                op = "subscribe",
                args = GetTopics(symbol)
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
        Send(webSocket,
            new
            {
                op = "unsubscribe",
                args = GetTopics(symbol)
            }
        );

        return true;
    }

    private List<string> GetTopics(Symbol symbol)
    {
        var brokerageSymbol = _symbolMapper.GetBrokerageSymbol(symbol);
        var category = GetBybitProductCategory(symbol);
        var depthString = GetDefaultOrderBookDepth(category);

        var topics = new List<string>
        {
            $"publicTrade.{brokerageSymbol}",
            $"orderbook.{depthString}.{brokerageSymbol}"
        };

        // This is required for open interest
        if (category != BybitProductCategory.Spot)
        {
            topics.Add($"tickers.{brokerageSymbol}");
        }

        return topics;
    }

    private void Connect(BybitApi api)
    {
        if (WebSocket == null) return;

        WebSocket.Initialize(_privateWebSocketUrl);

        // When connect is called from the api client factory the ApiClient instance property is not set yet
        api ??= ApiClient;

        if (!IsAccountMarginStatusValid(api, out var message))
        {
            OnMessage(message);
            return;
        }

        ConnectSync();

        // The initial authentication is done in sync to interrupt the brokerage initialization as this is a hard error
        if (!AuthenticatePrivateWSSync(api))
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

    private bool AuthenticatePrivateWSSync(BybitApi api)
    {
        using var resetEvent = new ManualResetEvent(false);
        var authenticated = false;

        void OnAuthenticated(object _, StreamAuthenticatedEventArgs args)
        {
            authenticated = args.IsAuthenticated;
            resetEvent.Set();
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

    private static string GetDefaultOrderBookDepth(BybitProductCategory category)
    {
        return category switch
        {
            BybitProductCategory.Inverse => "1",
            BybitProductCategory.Linear => "1",
            BybitProductCategory.Spot => "1",
            BybitProductCategory.Option => "25",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }

    private static bool IsAccountMarginStatusValid(BybitApi api, out BrokerageMessageEvent message)
    {
        var accountInfo = api.Account.GetAccountInfo();
        if (accountInfo.UnifiedMarginStatus == AccountUnifiedMarginStatus.ClassicAccount)
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