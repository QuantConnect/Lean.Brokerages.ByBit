using System.Linq;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit account api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/account/wallet-balance"/>
/// </summary>
public class BybitAccountApi : BybitBaseApi
{
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitAccountApi"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitAccountApi(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider, BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
    {
    }
        
    /// <summary>
    /// Obtain wallet balance, query asset information of each currency, and account risk rate information
    /// </summary>
    /// <param name="category">The product category</param>
    /// <returns>The wallet balances</returns>
    public BybitBalance GetWalletBalances(BybitProductCategory category)
    {
        var endpoint = $"{ApiPrefix}/account/wallet-balance";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("accountType", "UNIFIED");

      
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var balance =  EnsureSuccessAndParse<BybitPageResult<BybitBalance>>(response).List.Single();
        return balance;
    }

    /// <summary>
    /// Query the margin mode configuration of the account.
    /// </summary>
    /// <returns>The account info</returns>
    public BybitAccountInfo GetAccountInfo()
    {
        var endpoint = $"{ApiPrefix}/account/info";
        var request = new RestRequest(endpoint);
        
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitAccountInfo>(response);
    }
}