using System;
using QuantConnect.Brokerages;

namespace QuantConnect.BybitBrokerage;

public class BybitWebSocketWrapper : WebSocketClientWrapper
{
    public string ConnectionId { get; }
    public IConnectionHandler ConnectionHandler { get; }

    public BybitWebSocketWrapper(IConnectionHandler connectionHandler)
    {
        ConnectionId = Guid.NewGuid().ToString();
        ConnectionHandler = connectionHandler;
    }
}