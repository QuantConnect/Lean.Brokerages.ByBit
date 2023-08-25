using System;
using System.Collections.Generic;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitPositionApiClient : BybitRestApiClient
{
    
    public BybitPositionApiClient(ISymbolMapper symbolMapper, string apiPrefix, IRestClient restClient, ISecurityProvider securityProvider, Action<IRestRequest> requestAuthenticator) : base(symbolMapper, apiPrefix, restClient, securityProvider, requestAuthenticator)
    {
    }
    
    public IEnumerable<BybitPositionInfo> GetPositions(BybitAccountCategory category)
    {
        if (category == BybitAccountCategory.Spot) return Array.Empty<BybitPositionInfo>();
    
            //result => result.List.Length < 200
        return FetchAll(category, FetchPositionInfo);
    }
    
    private BybitPageResult<BybitPositionInfo> FetchPositionInfo(BybitAccountCategory category,string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/position/list";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("settleCoin", "USDT"); //todo
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