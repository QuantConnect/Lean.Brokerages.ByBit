namespace QuantConnect.BybitBrokerage.Models.Messages;

/// <summary>
/// Websocket message type
/// </summary>
public enum BybitMessageType
{
    /// <summary>
    /// Snapshot contains a full snapshot of the data
    /// </summary>
    Snapshot,
    /// <summary>
    /// Dela only contains field which were updated
    /// </summary>
    Delta
}