/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

namespace QuantConnect.Brokerages.Bybit.Models.Enums;

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