namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Cancel type
/// </summary>
public enum CancelType
{
    /// <summary>
    /// Cancelled by user
    /// </summary>
    CancelByUser,
    
    /// <summary>
    /// Cancelled by reduce-only
    /// </summary>
    CancelByReduceOnly,
    /// <summary>
    /// Cancelled due to liquidation
    /// </summary>
    CancelByPrepareLiq,
    /// <summary>
    /// Cancelled due to liquidation
    /// </summary>
    CancelAllBeforeLiq,
    /// <summary>
    /// Cancelled due to ADL
    /// </summary>
    CancelByPrepareAdl,
    /// <summary>
    /// Cancelled due to ADL
    /// </summary>
    CancelAllBeforeAdl,
    /// <summary>
    /// Cancelled by admin
    /// </summary>
    CancelByAdmin,
    /// <summary>
    /// Cancelled by TP/SL clear
    /// </summary>
    CancelByTpSlTsClear,
    /// <summary>
    /// Cancelled by pz. side change
    /// </summary>
    CancelByPzSideCh,
    /// <summary>
    /// Cancelled by SMP
    /// </summary>
    CancelBySmp,
    
    /// <summary>
    /// [Options] Cancelled by settle
    /// </summary>
    CancelBySettle,
    /// <summary>
    /// [Options] Cancelled by cannot afford order cost
    /// </summary>
    CancelByCannotAffordOrderCost,
    /// <summary>
    /// [Options] Cancelled by pm trial market-maker over equity
    /// </summary>
    CancelByPmTrialMmOverEquity,
    /// <summary>
    /// [Options] Cancelled by account blocking
    /// </summary>
    CancelByAccountBlocking,
    /// <summary>
    /// [Options] Cancelled by delivery
    /// </summary>
    CancelByDelivery,
    /// <summary>
    /// [Options] Cancelled by market-maker protection 
    /// </summary>
    CancelByMmpTriggered,
    /// <summary>
    /// [Options] Cancelled by cross self much
    /// </summary>
    CancelByCrossSelfMuch,
    
    /// <summary>
    /// [Options] Cancelled by cross reach max trades 
    /// </summary>
    CancelByCrossReachMaxTradeNum,
    /// <summary>
    /// [Options] Cancelled by disconnect protection 
    /// </summary>
    CancelByDCP,
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown
}