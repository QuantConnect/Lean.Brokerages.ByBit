using NUnit.Framework;
using QuantConnect.Interfaces;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage.Tests;

[TestFixture]
public class BybitFuturesBrokerageFactoryTests
{
    [Test]
    public void InitializesFactoryFromComposer()
    {
        using var factory = Composer.Instance.Single<IBrokerageFactory>(instance => instance.BrokerageType == typeof(BybitFuturesBrokerage));
        Assert.IsNotNull(factory);
    }
}