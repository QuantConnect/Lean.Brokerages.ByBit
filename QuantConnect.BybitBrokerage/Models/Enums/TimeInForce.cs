namespace QuantConnect.BybitBrokerage.Models.Enums;

public enum TimeInForce
{
    /// <summary>
    /// Good till Cancel
    /// </summary>
    GTC,
    /// <summary>
    /// Immediate or Cancel
    /// </summary>
    IOC,
    /// <summary>
    /// Fill or Kill
    /// </summary>
    FOK,
    /// <summary>
    /// Post Only <seealso href="https://www.bybit.com/en-US/help-center/bybitHC_Article?language=en_US&id=000001051"/>
    /// </summary>
    PostOnly
}