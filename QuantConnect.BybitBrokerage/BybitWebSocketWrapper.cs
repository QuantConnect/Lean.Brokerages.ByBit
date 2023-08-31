using System;
using System.Timers;
using QuantConnect.Brokerages;

namespace QuantConnect.BybitBrokerage;

public class BybitWebSocketWrapper : WebSocketClientWrapper
{
    private Timer _pingTimer;
    public string ConnectionId { get; }

    public BybitWebSocketWrapper()
    {
        ConnectionId = Guid.NewGuid().ToString();
    }

    private void PingTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Send("{\"op\":\"ping\"}");
    }

    protected override void OnOpen()
    {
        _pingTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
        _pingTimer.Elapsed += PingTimerElapsed;
        _pingTimer.Start();
        base.OnOpen();
    }

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
}