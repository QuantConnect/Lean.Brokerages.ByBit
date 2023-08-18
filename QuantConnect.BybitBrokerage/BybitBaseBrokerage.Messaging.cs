using System;
using Newtonsoft.Json;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Logging;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage;

public partial class BybitBrokerage
{

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

    private void OnDataMessage(WebSocketMessage webSocketMessage)
    {
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