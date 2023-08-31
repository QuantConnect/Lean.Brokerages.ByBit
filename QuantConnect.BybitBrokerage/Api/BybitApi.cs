

using System;
using System.Collections.Generic;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Provides methods to work with the Bybit rest API
/// </summary>
public class BybitApi : IDisposable
{
    private static readonly Dictionary<BybitVIPLevel, int> RateLimits = new()
    {
        { BybitVIPLevel.VIP0 , 10},
        { BybitVIPLevel.VIP1 , 20},
        { BybitVIPLevel.VIP2 , 40},
        { BybitVIPLevel.VIP3 , 60},
        { BybitVIPLevel.VIP4 , 60},
        { BybitVIPLevel.VIP5 , 60},
        { BybitVIPLevel.SupremeVIP , 60},
        { BybitVIPLevel.Pro1 , 100},
        { BybitVIPLevel.Pro2 , 150},
        { BybitVIPLevel.Pro3 , 200},
        { BybitVIPLevel.Pro4 , 200},
        { BybitVIPLevel.Pro5 , 200}
    };

    private readonly BybitApiClient _apiClient;
    private readonly string _apiKey;
    
    /// <summary>
    /// Market Api
    /// </summary>
    public BybitMarketApi Market { get; }
    
    /// <summary>
    /// Account Api
    /// </summary>
    public BybitAccountApi Account { get; }
    
    /// <summary>
    /// Position Api
    /// </summary>
    public BybitPositionApi Position { get; }
    
    /// <summary>
    /// Trade Api
    /// </summary>
    public BybitTradeApi Trade { get; }

    
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitApi"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="restApiUrl">The api url</param>
    /// <param name="maxRequestsPerSecond">The request limit per second</param>
    public BybitApi(ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl,
        int maxRequestsPerSecond)
    {
        
        _apiKey = apiKey;
        _apiClient = new BybitApiClient(apiKey, apiSecret, restApiUrl,maxRequestsPerSecond);
        
        const string apiPrefix = "/v5";
        Market = new BybitMarketApi(symbolMapper, apiPrefix, securityProvider, _apiClient);
        Account = new BybitAccountApi(symbolMapper, apiPrefix, securityProvider,_apiClient );
        Position = new BybitPositionApi(symbolMapper, apiPrefix, securityProvider, _apiClient);
        Trade = new BybitTradeApi(Market, symbolMapper, apiPrefix, securityProvider,_apiClient);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitApi"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="restApiUrl">The api url</param>
    /// <param name="vipLevel">The accounts VIP level</param>
    public BybitApi(
        ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl,
        BybitVIPLevel vipLevel = BybitVIPLevel.VIP0) :this(symbolMapper,securityProvider,apiKey, apiSecret, restApiUrl, RateLimits.GetValueOrDefault(vipLevel,10))
    {

    }
    
    /// <summary>
    /// Returns a websocket message which can be used to authenticate the private socket connection
    /// </summary>
    /// <returns></returns>
    public object AuthenticateWebSocket()
    {
        var expires = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeMilliseconds();
        var signString = $"GET/realtime{expires}";
        var signed = _apiClient.Sign(signString);
        return new
        {
            op = "auth",
            args = new object[]
            {
                _apiKey,
                expires,
                signed
            }
        };
    }
    
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _apiClient.Dispose();
    }
}