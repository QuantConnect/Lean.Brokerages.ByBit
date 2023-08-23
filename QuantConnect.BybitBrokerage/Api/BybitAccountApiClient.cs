using System;
using System.Linq;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitAccountApiClient : BybitRestApiClient
{
    
    public BybitAccountApiClient(ISymbolMapper symbolMapper, string apiPrefix, IRestClient restClient, Action<IRestRequest> requestAuthenticator) : base(symbolMapper, apiPrefix, restClient, requestAuthenticator)
    {
    }
        
    public BybitBalance GetWalletBalances(BybitAccountCategory category)
    {
        var endpoint = $"{ApiPrefix}/account/wallet-balance";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("accountType", category == BybitAccountCategory.Inverse ? "CONTRACT":"UNIFIED");

      
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var balance =  EnsureSuccessAndParse<BybitPageResult<BybitBalance>>(response).List.Single();
        return balance;
    }


}