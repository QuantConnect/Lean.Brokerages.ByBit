using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models;
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

    // Binance allows 5 messages per second, but we still get rate limited if we send a lot of messages at that rate
    // By sending 3 messages per second, evenly spaced out, we can keep sending messages without being limited
    //todo
    private readonly RateGate _webSocketRateLimiter = new RateGate(1, TimeSpan.FromMilliseconds(330));


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
            var topic = jObj.Value<string>("topic");
            switch (topic)
            {
                case "order":
                    HandleOrderUpdate(jObj);
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

    private void HandleOrderUpdate(JObject jObject)
    {
        var orders = jObject.ToObject<BybitDataMessage<BybitOrder[]>>(JsonSerializer).Data;
        foreach (var order in orders)
        {
            var leanOrder = OrderProvider.GetOrdersByBrokerageId(order.OrderId).FirstOrDefault();

            //todo why multiplied orders here? var leanOrder = leanOrders.FirstOrDefault;
            // test orderprovider is cloning orders, therefore the market order test never is filled and waits for an event which already fired
            if (leanOrder == null) continue;
            
            var newStatus = ConvertOrderStatus(order.Status);
            var orderEvent = default(OrderEvent);

            //todo should fees be sent as cumulative fees or fees per execution? (partial fills)
            var fee = OrderFee.Zero;
            if (order.QuantityFilled.HasValue && order.ExecutedFee.HasValue)
            {
                CurrencyPairUtil.DecomposeCurrencyPair(leanOrder.Symbol, out var baseCurrency, out _);
                fee = new OrderFee(new CashAmount(order.ExecutedFee.Value, baseCurrency));
            }

            orderEvent = new OrderEvent(leanOrder, order.UpdateTime, fee) { Status = newStatus };
            OnOrderEvent(orderEvent);
        }
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

        _webSocketRateLimiter.WaitToProceed();

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
}