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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Packets;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage;

public partial class BybitBrokerage
{
    private IDataAggregator _aggregator;

    /// <summary>
    /// Subscribe to the specified configuration
    /// </summary>
    /// <param name="dataConfig">defines the parameters to subscribe to a data feed</param>
    /// <param name="newDataAvailableHandler">handler to be fired on new data available</param>
    /// <returns>The new enumerator for this subscription request</returns>
    public IEnumerator<BaseData> Subscribe(SubscriptionDataConfig dataConfig, EventHandler newDataAvailableHandler)
    {
        if (!CanSubscribe(dataConfig.Symbol))
        {
            return null;
        }

        var enumerator = _aggregator.Add(dataConfig, newDataAvailableHandler);
        GetSubscriptionManager(dataConfig.Symbol).Subscribe(dataConfig);

        return enumerator;
    }

    /// <summary>
    /// Removes the specified configuration
    /// </summary>
    /// <param name="dataConfig">Subscription config to be removed</param>
    public void Unsubscribe(SubscriptionDataConfig dataConfig)
    {
        GetSubscriptionManager(dataConfig.Symbol).Unsubscribe(dataConfig);
        _aggregator.Remove(dataConfig);
    }

    /// <summary>
    /// Sets the job we're subscribing for
    /// </summary>
    /// <param name="job">The job we're subscribing for</param>
    public void SetJob(LiveNodePacket job)
    {
        var aggregator = Composer.Instance.GetExportedValueByTypeName<IDataAggregator>(
            Config.Get("data-aggregator", "QuantConnect.Lean.Engine.DataFeeds.AggregationManager"),
            forceTypeNameOnExisting: false);

        SetJobInit(job, aggregator);

        if (!IsConnected)
        {
            Connect();
        }
    }

    /// <summary>
    /// Initializes the brokerage for the job we're subscribing for
    /// </summary>
    /// <param name="job">The job we're subscribing for</param>
    /// <param name="aggregator">The aggregator for consolidating ticks</param>
    protected virtual void SetJobInit(LiveNodePacket job, IDataAggregator aggregator)
    {
        var vipLevelString = job.BrokerageData["bybit-vip-level"];
        var vipLevel = Enum.Parse<BybitVIPLevel>(vipLevelString, true);

        Initialize(
            baseWssUrl: job.BrokerageData["bybit-websocket-url"],
            restApiUrl: job.BrokerageData["bybit-api-url"],
            apiKey: job.BrokerageData["bybit-api-key"],
            apiSecret: job.BrokerageData["bybit-api-secret"],
            algorithm: null,
            orderProvider: null,
            securityProvider: null,
            aggregator,
            job,
            Market.Bybit,
            vipLevel
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DataQueueHandlerSubscriptionManager GetSubscriptionManager(Symbol symbol) => _subscriptionManagers[GetBybitProductCategory(symbol)];
}