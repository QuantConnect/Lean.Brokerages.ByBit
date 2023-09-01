using System;
using System.Collections.Generic;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit position api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/position"/>
/// </summary>
public class BybitPositionApiEndpoint : BybitApiEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitPositionApiEndpoint"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitPositionApiEndpoint(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider,
        BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
    {
    }

    /// <summary>
    /// Query real-time position data, such as position size, cumulative realizedPNL.
    /// </summary>
    /// <param name="category">The product category</param>
    /// <returns>A list of all open positions in the current category</returns>
    public IEnumerable<BybitPositionInfo> GetPositions(BybitProductCategory category)
    {
        if (category == BybitProductCategory.Spot) return Array.Empty<BybitPositionInfo>();

        var parameters = new KeyValuePair<string, string>[]
        {
            new("settleCoin", "USDT")
        };
        return FetchAll<BybitPositionInfo>("/position/list", category, 200, parameters, true);
    }
}