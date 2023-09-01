using System.Collections.Generic;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit account api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/account/wallet-balance"/>
/// </summary>
public class BybitAccountApiEndpoint : BybitApiEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitAccountApiEndpoint"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitAccountApiEndpoint(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider,
        BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
    {
    }

    /// <summary>
    /// Obtain wallet balance, query asset information of each currency, and account risk rate information
    /// </summary>
    /// <param name="category">The product category</param>
    /// <returns>The wallet balances</returns>
    public BybitBalance GetWalletBalances(BybitProductCategory category)
    {
        var parameters = new KeyValuePair<string, string>[]
        {
            new("accountType", "UNIFIED")
        };

        var result =
            ExecuteGetRequest<BybitPageResult<BybitBalance>>("/account/wallet-balance", category, parameters, true);

        return result.List[0];
    }

    /// <summary>
    /// Query the margin mode configuration of the account.
    /// </summary>
    /// <returns>The account info</returns>
    public BybitAccountInfo GetAccountInfo()
    {
        return ExecuteGetRequest<BybitAccountInfo>("/account/info", authenticate: true);
    }
}