namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Trade execution type
/// </summary>
public enum ExecutionType
{
    /// <summary>
    /// Trade
    /// </summary>
    Trade,
    /// <summary>
    /// Auto-Deleveraging <seealso href="https://www.bybit.com/en-US/help-center/bybitHC_Article?language=en_US&id=000001124"/>
    /// </summary>
    AdlTrade,
    /// <summary>
    /// Funding fee <seealso href="https://www.bybit.com/en-US/help-center/HelpCenterKnowledge/bybitHC_Article?id=000001123&language=en_US"/>
    /// </summary>
    Funding,
    /// <summary>
    /// Liquidation
    /// </summary>
    BustTrade,
    /// <summary>
    /// USDC Futures delivery
    /// </summary>
    Delivery,
    /// <summary>
    /// Block trade
    /// </summary>
    BlockTrade
}