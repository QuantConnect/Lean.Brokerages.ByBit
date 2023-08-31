namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum TickDirection
{
    /// <summary>
    /// Price rise
    /// </summary>
    PlusTick,
    /// <summary>
    /// Trade occured at the same price as the previous trade, which occurred at a price higher than that for the trade preceding it
    /// </summary>
    ZeroPlusTick,
    /// <summary>
    /// Price drop
    /// </summary>
    MinusTick,
    /// <summary>
    /// Trade occured at the same price as the previous trade, which occurred at a price lower than that for the trade preceding it
    /// </summary>
    ZeroMinusTick
}