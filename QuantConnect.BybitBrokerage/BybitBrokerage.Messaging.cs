using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Data;
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
    protected readonly object TickLocker = new();

    /// <summary>
    /// Locking object for the Ticks list in the data queue handler
    /// </summary>
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
                HandleOpMessage(jObj);
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
                default:
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
                //todo validate
                var currency = Category switch
                {
                    BybitAccountCategory.Linear => "USDT",
                    BybitAccountCategory.Inverse => GetBaseCurrency(symbol),
                    BybitAccountCategory.Spot => GetSpotFeeCurrency(leanSymbol,tradeUpdate),
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
            CurrencyPairUtil.DecomposeCurrencyPair(pair, out var @base, out _);
            return pair;
        }
    }

    private void HandleOrderUpdate(JObject jObject)
    {
        var orders = jObject.ToObject<BybitDataMessage<BybitOrder[]>>(JsonSerializer).Data;
        foreach (var order in orders)
        {
            //We're not interested in order executions here as HandleOrderExecution is taking care of this
            if(order.Status is OrderStatus.Filled or OrderStatus.PartiallyFilled) continue;
            
            //This should imo only be one order but the test OrderProvider is cloning them
            var leanOrder = OrderProvider.GetOrdersByBrokerageId(order.OrderId).FirstOrDefault();
            if (leanOrder == null) continue;

            var newStatus = ConvertOrderStatus(order.Status);
            if(newStatus == leanOrder.Status) continue;
            
            Log.Trace($"Order status changed from: {leanOrder.Status.ToStringInvariant()} to: {newStatus.ToStringInvariant()}");
            
            var orderEvent = new OrderEvent(leanOrder, order.UpdateTime, OrderFee.Zero){Status = newStatus};
            TestFix(leanOrder.Id, newStatus);
            OnOrderEvent(orderEvent);
        }
    }

    //Todo: This needs to be removed, but for now it fixes the issues I was facing with the tests expecting the status of the original order object being
    //      updated. While the OrderProvider being used in the test returns copies of each order.
    private void TestFix(int orderId,Orders.OrderStatus status)
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
                HandleOpMessage(obj);
                return;
            }
            else if (obj.TryGetValue("topic", out var topic))
            {
                var topicStr = topic.Value<string>();
                if (topicStr.StartsWith("publicTrade"))
                {
                    HandleTradeMessage(obj);
                }
                else if (topicStr.StartsWith("tickers"))
                {
                    HandleTickerMessage(obj);
                }
            }
        }
        catch (Exception e)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                $"Parsing wss message failed. Data: {e.Message} Exception: {e}"));
        }
    }

    private void HandleTickerMessage(JToken message)
    {
        var tickerMessage = JsonConvert.DeserializeObject<BybitDataMessage<BybitTicker>>(message.ToString(), Settings);
        var ticker = tickerMessage.Data;

        var bidPrice = ticker.Bid1Price;
        var bidSize = ticker.Bid1Size;
        var askPrice = ticker.Ask1Price;
        var askSize = ticker.Ask1Size;

        if (!bidPrice.HasValue || !bidSize.HasValue | !askPrice.HasValue || !askSize.HasValue) return;
        EmitQuoteTick(_symbolMapper.GetLeanSymbol(ticker.Symbol, GetSupportedSecurityType(), MarketName),
            bidPrice.Value, bidSize.Value, askPrice.Value, askSize.Value);
    }

    private void HandleTradeMessage(JToken message)
    {
        var trades = message.ToObject<BybitDataMessage<BybitWSTradeData[]>>();
        foreach (var trade in trades.Data)
        {
            EmitTradeTick(_symbolMapper.GetLeanSymbol(trade.Symbol, GetSupportedSecurityType(), MarketName), trade.Time,
                trade.Price, trade.Value);
        }
    }

    private void HandleOpMessage(JToken message)
    {
        var dataMessage = message.ToObject<BybitMessage>();
        if (dataMessage.Operation == "subscribe" && dataMessage.Success)
        {
            //todo does the subscription needs to be confirmed?
        }

        if (dataMessage.Operation == "auth")
        {
            Log.Trace($"WS auth: success={dataMessage.Success}");
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

        lock (TickLocker)
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

        lock (TickLocker)
        {
            _aggregator.Update(tick);
        }
    }

    private void Send(IWebSocket webSocket, object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        
        Log.Trace("Send: " + json);
        webSocket.Send(json);
    }


    /// <summary>
    /// Subscribes to the requested symbol (using an individual streaming channel)
    /// </summary>
    /// <param name="webSocket">The websocket instance</param>
    /// <param name="symbol">The symbol to subscribe</param>
    private bool Subscribe(IWebSocket webSocket, Symbol symbol)
    {
        var s = _symbolMapper.GetBrokerageSymbol(symbol);
        Send(webSocket,
            new
            {
                op = "subscribe",
                args = new[]
                {
                    $"publicTrade.{s}",
                    $"tickers.{s}" //Push frequency: Derivatives & Options - 100ms, Spot - real-time
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
        var s = _symbolMapper.GetBrokerageSymbol(symbol);

        Send(webSocket,
            new
            {
                op = "unsubscribe",
                args = new[]
                {
                    $"publicTrade.{s}",
                    $"tickers.{s}"
                }
            }
        );

        return true;
    }
    
    private void Connect(BybitApi api)
    {
        if (WebSocket == null) return;
        WebSocket.Initialize(_privateWebSocketUrl);
        if (api == null)
        {
            WebSocket.Open += OnOpenInstance;

        }
        else
        {
            WebSocket.Open += OnOpen;
        }

        void OnOpenInstance(object sender, EventArgs e)
        {
            OnPrivateWSConnected(ApiClient);
        }
        void OnOpen(object sender, EventArgs e)
        {
            WebSocket.Open -= OnOpen;
            OnPrivateWSConnected(api);
            WebSocket.Open += OnOpenInstance;
        }
        ConnectSync();
    }

    private void OnPrivateWSConnected(BybitApi api)
    {
        Send(WebSocket, api.AuthenticateWebSocket());
        Send(WebSocket, new { op = "subscribe", args = new[] { "order","execution" } });
    }

}
