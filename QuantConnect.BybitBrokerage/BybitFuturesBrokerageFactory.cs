using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage;

/// <summary>
/// Factory method to create Bybit Futures brokerage
/// </summary>
public class BybitFuturesBrokerageFactory : BybitBrokerageFactory
{
    /// <summary>
    /// The name of the config key representing the requested order book depth
    /// <seealso href="https://bybit-exchange.github.io/docs/v5/websocket/public/orderbook"/>
    /// </summary>
    protected override string OrderBookDepthConfigName => "bybit-futures-orderbook-depth";

    /// <summary>
    /// The default order book depth for this brokerage
    /// </summary>
    protected override int DefaultOrderBookDepth => 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="BybitFuturesBrokerageFactory"/> class
    /// </summary>
    public BybitFuturesBrokerageFactory() : base(typeof(BybitFuturesBrokerage))
    {
    }

    /// <summary>
    /// Gets a brokerage model that can be used to model this brokerage's unique behaviors
    /// </summary>
    /// <param name="orderProvider">The order provider</param>
    public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new BybitFuturesBrokerageModel();

    /// <summary>
    /// Creates a new IBrokerage instance
    /// </summary>
    /// <param name="job">The job packet to create the brokerage for</param>
    /// <param name="algorithm">The algorithm instance</param>
    /// <param name="aggregator">The aggregator for consolidating ticks</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="apiUrl">The rest api url</param>
    /// <param name="wsUrl">The websocket base url</param>
    /// <param name="orderBookDepth">The requested order book depth</param>
    /// <param name="vipLevel">Bybit VIP level</param>
    /// <returns>A new brokerage instance</returns>
    protected override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm, IDataAggregator aggregator,
        string apiKey,
        string apiSecret, string apiUrl, string wsUrl, BybitVIPLevel vipLevel, int orderBookDepth)
    {
        return new BybitFuturesBrokerage(apiKey, apiSecret, apiUrl, wsUrl, algorithm, aggregator, job, orderBookDepth,
            vipLevel);
    }
}