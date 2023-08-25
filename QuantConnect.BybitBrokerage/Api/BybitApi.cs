using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QuantConnect.Brokerages;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitApi : IDisposable
{    
    private readonly HMACSHA256 _hmacsha256;
    private readonly string _apiKey;
    private readonly RestClient _restClient;

    public BybitMarketApiClient Market { get; }
    public BybitAccountApiClient Account { get; }
    public BybitPositionApiClient Position { get; }
    public BybitTradeApiClient Trade { get; }
    public BybitApi(
        ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl)
    {
        _restClient = new RestClient(restApiUrl);
        _apiKey = apiKey;
        _hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret ?? string.Empty));

        var apiPrefix = "/v5";

        Market = new BybitMarketApiClient(symbolMapper, apiPrefix, _restClient,securityProvider, AuthenticateRequest);
        Account = new BybitAccountApiClient(symbolMapper, apiPrefix, _restClient,securityProvider, AuthenticateRequest);
        Position = new BybitPositionApiClient(symbolMapper, apiPrefix, _restClient,securityProvider, AuthenticateRequest);
        Trade = new BybitTradeApiClient(Market, symbolMapper, apiPrefix, _restClient,securityProvider, AuthenticateRequest);
        
    }
    public object AuthenticateWebSocket()
    {
        var expires = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeMilliseconds();
        var signString = $"GET/realtime{expires}";
        var signed = Sign(signString, _hmacsha256);
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
        _hmacsha256.Dispose();
    }

    private void AuthenticateRequest(IRestRequest request)
    {
        string sign;
        if (request.Method == Method.GET)
        {
            var queryParams = request.Parameters
                .Where(x => x.Type is ParameterType.QueryString or ParameterType.QueryStringWithoutEncode)
                .Select(x => $"{x.Name}={x.Value}")
                .ToArray();

            sign = string.Join("&", queryParams);
            

        }else if (request.Method == Method.POST)
        {
            var body = request.Parameters.Single(x => x.Type == ParameterType.RequestBody).Value;
            sign = (string)body;
        }
        else
        {
            throw new NotSupportedException();
        }
        
        var nonce = GetNonce();
        var sToSign = $"{nonce}{_apiKey}{sign}";
        var signed = Sign(sToSign, _hmacsha256);
        request.AddHeader("X-BAPI-SIGN", signed);
        request.AddHeader("X-BAPI-API-KEY", _apiKey);
        request.AddHeader("X-BAPI-TIMESTAMP", nonce);
        request.AddHeader("X-BAPI-SIGN-TYPE", "2");
    }
    private static string Sign(string queryString, HMACSHA256 hmacsha256)
    {
        var messageBytes = Encoding.UTF8.GetBytes(queryString);

        var computedHash = hmacsha256.ComputeHash(messageBytes);
        var hex = new StringBuilder(computedHash.Length * 2);
        foreach (var b in computedHash)
        {
            hex.Append($"{b:x2}");
        }

        return hex.ToString();
    }

    private static string GetNonce()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
    }
    
}