using System;
using System.Collections.Generic;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitPositionApiClient : BybitRestApiClient
{
    
    public BybitPositionApiClient(ISymbolMapper symbolMapper, string apiPrefix, IRestClient restClient, Action<IRestRequest> requestAuthenticator) : base(symbolMapper, apiPrefix, restClient, requestAuthenticator)
    {
    }
    
    public IEnumerable<BybitPositionInfo> GetPositions(BybitAccountCategory category)
    {
        return FetchAll(category, FetchPositionInfo, result => result.List.Length < 200);
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