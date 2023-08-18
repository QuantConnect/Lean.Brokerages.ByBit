using System;
using System.Collections.Generic;
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
        _subscriptionManager.Subscribe(dataConfig);

        return enumerator;
    }

    /// <summary>
    /// Removes the specified configuration
    /// </summary>
    /// <param name="dataConfig">Subscription config to be removed</param>
    public void Unsubscribe(SubscriptionDataConfig dataConfig)
    {
        _subscriptionManager.Unsubscribe(dataConfig);
        _aggregator.Remove(dataConfig);
    }

    /// <summary>
    /// Sets the job we're subscribing for
    /// </summary>
    /// <param name="job">Job we're subscribing for</param>
    public void SetJob(LiveNodePacket job)
    {
        var aggregator = Composer.Instance.GetExportedValueByTypeName<IDataAggregator>(
            Config.Get("data-aggregator", "QuantConnect.Lean.Engine.DataFeeds.AggregationManager"), forceTypeNameOnExisting: false);
    
        SetJobInit(job, aggregator);
    
        if (!IsConnected)
        {
            Connect();
        }
    }
    
    protected virtual void SetJobInit(LiveNodePacket job, IDataAggregator aggregator)
    {
        Initialize(
            wssUrl: job.BrokerageData["bybit-websocket-url"],
            restApiUrl: job.BrokerageData["bybit-api-url"],
            apiKey: job.BrokerageData["bybit-api-key"],
            apiSecret: job.BrokerageData["bybit-api-secret"],
            algorithm: null,
            aggregator,
            job,
            Market.Bybit
        );
    }
    
}