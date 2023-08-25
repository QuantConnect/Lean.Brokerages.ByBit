using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage;

public class BybitFuturesBrokerageFactory : BybitBrokerageFactory
{
    
    public BybitFuturesBrokerageFactory(): base(typeof(BybitFuturesBrokerage)){}

    public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new BybitFuturesBrokerageModel();

    protected override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm, IDataAggregator aggregator, string apiKey,
        string apiSecret, string apiUrl, string wsUrl)
    {
        return new BybitFuturesBrokerage(apiKey, apiSecret, apiUrl, wsUrl, algorithm, aggregator, job);

    }
}