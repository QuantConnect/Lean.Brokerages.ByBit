using System;
using System.Collections.Generic;
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
        SubscriptionManager.Subscribe(dataConfig);

        return enumerator;
    }

    /// <summary>
    /// Removes the specified configuration
    /// </summary>
    /// <param name="dataConfig">Subscription config to be removed</param>
    public void Unsubscribe(SubscriptionDataConfig dataConfig)
    {
        SubscriptionManager.Unsubscribe(dataConfig);
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
            int.Parse(job.BrokerageData[GetOrderBookDepthConfigName()]),
            vipLevel
        );
    }

    private string GetOrderBookDepthConfigName()
    {
        switch (Category)
        {
            case BybitProductCategory.Spot:
                return "bybit-orderbook-depth";
            case BybitProductCategory.Linear:
            case BybitProductCategory.Inverse:
                return "bybit-futures-orderbook-depth";
            case BybitProductCategory.Option:
                return "bybit-options-orderbook-depth";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}