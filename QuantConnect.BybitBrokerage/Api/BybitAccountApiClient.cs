﻿using System;
using System.Linq;
using Newtonsoft.Json;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.BybitBrokerage.Utility;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitAccountApiClient : BybitRestApiClient
{
    
    public BybitAccountApiClient(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider,  Func<IRestRequest, IRestResponse> executeRequest,Action<IRestRequest> requestAuthenticator) : base(symbolMapper, apiPrefix, securityProvider,executeRequest, requestAuthenticator)
    {
    }
        
    public BybitBalance GetWalletBalances(BybitAccountCategory category)
    {
        var endpoint = $"{ApiPrefix}/account/wallet-balance";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("accountType", EnumUtility.GetMemberValue(BybitAccountType(category)));

      
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var balance =  EnsureSuccessAndParse<BybitPageResult<BybitBalance>>(response).List.Single();
        return balance;
    }

    public BybitAccountInfo GetAccountInfo()
    {
        var endpoint = $"{ApiPrefix}/account/info";
        var request = new RestRequest(endpoint);
        
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitAccountInfo>(response);
    }
}