using System;
using System.Timers;
using QuantConnect.Brokerages;

namespace QuantConnect.BybitBrokerage;

/// <summary>
/// Wrapper class for a Bybit websocket connection
/// </summary>
public class BybitWebSocketWrapper : WebSocketClientWrapper
{
    private Timer _pingTimer;
    
    /// <summary>
    /// The unique id for the connection
    /// </summary>
    public string ConnectionId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BybitWebSocketWrapper"/> class
    /// </summary>
    public BybitWebSocketWrapper()
    {
        ConnectionId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Event invocator for the <see cref="WebSocketClientWrapper.Open"/> event
    /// </summary>
    protected override void OnOpen()
    {
        _pingTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
        _pingTimer.Elapsed += PingTimerElapsed;
        _pingTimer.Start();
        base.OnOpen();
    }

    /// <summary>
    /// Event invocator for the <see cref="WebSocketClientWrapper.Close"/> event
    /// </summary>
    protected override void OnClose(WebSocketCloseData e)
    {
        if (_pingTimer != null)
        {
            _pingTimer.Stop();
            _pingTimer.Elapsed -= PingTimerElapsed;
            _pingTimer.Dispose();
            _pingTimer = null;
        }

        base.OnClose(e);
    }
    
    
    private void PingTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Send("{\"op\":\"ping\"}");
    }
}