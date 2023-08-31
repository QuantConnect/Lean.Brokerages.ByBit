using System;
using System.Collections.Generic;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;


/// <summary>
/// Bybit position api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/position"/>
/// </summary>
public class BybitPositionApi : BybitBaseApi
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitPositionApi"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitPositionApi(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider, BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
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
    
            //result => result.List.Length < 200
        return FetchAll(category, FetchPositionInfo);
    }
    
    private BybitPageResult<BybitPositionInfo> FetchPositionInfo(BybitProductCategory category,string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/position/list";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("settleCoin", "USDT"); //todo, needs to be changed for inverse
        request.AddQueryParameter("limit", "200");
        if (cursor != null)
        {
            request.AddQueryParameter("cursor", cursor, false);
        }
        
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);
        return EnsureSuccessAndParse<BybitPageResult<BybitPositionInfo>>(response);
    }

}