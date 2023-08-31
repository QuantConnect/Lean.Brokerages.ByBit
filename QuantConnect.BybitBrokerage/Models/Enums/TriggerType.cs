namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Trigger type
/// </summary>
public enum TriggerType
{
    /// <summary>
    /// Last price
    /// </summary>
    LastPrice,
    
    /// <summary>
    /// Index proce
    /// </summary>
    IndexPrice,
    
    /// <summary>
    /// Mark price
    /// </summary>
    MarkPrice,
    
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown
}