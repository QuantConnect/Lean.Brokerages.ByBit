using System;
using QuantConnect.Brokerages;
using QuantConnect.Securities;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitApi : IDisposable
{
    private readonly BybitApiClient _apiClient;
    private readonly string _apiKey;
    public BybitMarketApi Market { get; }
    public BybitAccountApi Account { get; }
    public BybitPositionApi Position { get; }
    public BybitTradeApi Trade { get; }
    
    public BybitApi(
        ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl)
    {

        _apiKey = apiKey;
        _apiClient = new BybitApiClient(apiKey, apiSecret, restApiUrl);
        
        const string apiPrefix = "/v5";
        Market = new BybitMarketApi(symbolMapper, apiPrefix, securityProvider, _apiClient);
        Account = new BybitAccountApi(symbolMapper, apiPrefix, securityProvider,_apiClient );
        Position = new BybitPositionApi(symbolMapper, apiPrefix, securityProvider, _apiClient);
        Trade = new BybitTradeApi(Market, symbolMapper, apiPrefix, securityProvider,_apiClient);
    }
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
    public void Dispose()
    {
        _apiClient.DisposeSafely();
    }
}