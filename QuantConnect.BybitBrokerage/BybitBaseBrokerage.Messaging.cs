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

    private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter>() { new ByBitKlineJsonConverter(), new BybitDecimalStringConverter() },
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
    };

    // Binance allows 5 messages per second, but we still get rate limited if we send a lot of messages at that rate
    // By sending 3 messages per second, evenly spaced out, we can keep sending messages without being limited
    //todo
    private readonly RateGate _webSocketRateLimiter = new RateGate(1, TimeSpan.FromMilliseconds(330));
    protected readonly object TickLocker = new object();

    
    
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

            var jObj = JToken.Parse(e.Message);
            var topic = jObj.Value<string>("topic");
            if (topic == "order")
            {
                var orders = JsonConvert.DeserializeObject<BybitDataMessage<BybitOrder[]>>(jObj.ToString(), Settings).Data;
                foreach (var order in orders)
                {

                    var leanOrders = OrderProvider.GetOrdersByBrokerageId(order.OrderId).ToArray().Take(1);
                    foreach (var leanOrder in leanOrders)
                    {
                        //todo why multiplied orders here? var leanOrder = leanOrders.FirstOrDefault;
                        // test orderprovider is cloning orders, therefore the market order test never is filled and waits for an event which already fired
                        if (leanOrder == null) continue;

                        if (order.Status == OrderStatus.Filled)
                        {
                            leanOrder.Status = Orders.OrderStatus.Filled;
                            var fee = new OrderFee(new CashAmount(order.ExecutedFee ?? 0, "USDT"));
                            var @event = new OrderEvent(leanOrder.Id, leanOrder.Symbol, DateTime.UtcNow,
                                Orders.OrderStatus.Filled, leanOrder.Direction, order.Price ?? 0,
                                order.QuantityFilled ?? 0, fee);
                            OnOrderEvent(@event);
                        }
                        else if (order.Status is OrderStatus.Cancelled or OrderStatus.Deactivated)
                        {
                            leanOrder.Status = Orders.OrderStatus.Canceled;
                            OnOrderEvent(new OrderEvent(leanOrder, order.UpdateTime, OrderFee.Zero)
                                { Status = Orders.OrderStatus.Canceled });
                        }
                        else
                        {
                            Log.Trace(e.Message);
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1,
                $"Parsing wss message failed. Data: {e.Message} Exception: {exception}"));
            throw;
        }
    }

    private void Send(IWebSocket webSocket, object obj)
    {
        var json = JsonConvert.SerializeObject(obj);

        _webSocketRateLimiter.WaitToProceed();

        Log.Trace("Send: " + json);
        webSocket.Send(json);
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
            if (obj.TryGetValue("op", out var op))
            {
                var dataMessage = obj.ToObject<BybitMessage>();
                if (dataMessage.Operation == "subscribe" && dataMessage.Success)
                {
                    //todo
                }
                //todo on data message
            }else if (obj.TryGetValue("topic", out var topic))
            {
                var topicStr = topic.Value<string>();
                if (topicStr.StartsWith("publicTrade"))
                {
                    var trades = obj.ToObject<BybitDataMessage<BybitWSTradeData[]>>();
                    foreach (var trade in trades.Data)
                    {
                        EmitTradeTick(_symbolMapper.GetLeanSymbol(trade.Symbol,GetSupportedSecurityType(),MarketName),trade.Time,trade.Price,trade.Value);
                    }
                    
                    //order event toodo
                    
                }else if (topicStr.StartsWith("orderbook"))
                {
                    //todo
                    
                }
            }
        }
        catch (Exception e)
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1, $"Parsing wss message failed. Data: {e.Message} Exception: {e}"));

        }
        

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
                    $"orderbook.500.{s}"
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
                    $"orderbook_500.{s}"
                }
            }
        );

        return true;
    }
    

}