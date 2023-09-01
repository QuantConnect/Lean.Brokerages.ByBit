using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using QuantConnect.Util;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit api client implementation
/// </summary>
public class BybitApiClient : IDisposable
{
    private readonly HMACSHA256 _hmacSha256;
    private readonly string _apiKey;
    private readonly RestClient _restClient;
    private readonly RateGate _rateGate;

    /// <summary>
    /// Initializes a new instance of the <see cref="BybitApiClient"/> class 
    /// </summary>
    /// <param name="apiKey">The api key</param>
    /// <param name="apiSecret">The api secret</param>
    /// <param name="restApiUrl">The api url</param>
    /// <param name="maxRequestsPerSecond">The api rate limit per second</param>
    public BybitApiClient(
        string apiKey,
        string apiSecret,
        string restApiUrl,
        int maxRequestsPerSecond)
    {
        _restClient = new RestClient(restApiUrl);
        _apiKey = apiKey;
        _hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret ?? string.Empty));
        _rateGate = new RateGate(maxRequestsPerSecond, Time.OneSecond);
    }


    /// <summary>
    /// Authenticates the provided request by signing it and adding the required headers
    /// </summary>
    /// <param name="request">The request to authenticate</param>
    /// <exception cref="NotSupportedException">Is thrown when an unsupported request type is provided. Only GET and POST are supported</exception>
    public void AuthenticateRequest(IRestRequest request)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("Client needs to be created with api credentials to execute private endpoints");
        }

        string sign;
        if (request.Method == Method.GET)
        {
            var queryParams = request.Parameters
                .Where(x => x.Type is ParameterType.QueryString or ParameterType.QueryStringWithoutEncode)
                .Select(x => $"{x.Name}={x.Value}")
                .ToArray();

            sign = string.Join("&", queryParams);
        }
        else if (request.Method == Method.POST)
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
        var signed = Sign(sToSign);
        request.AddHeader("X-BAPI-SIGN", signed);
        request.AddHeader("X-BAPI-API-KEY", _apiKey);
        request.AddHeader("X-BAPI-TIMESTAMP", nonce);
        request.AddHeader("X-BAPI-SIGN-TYPE", "2");
    }

    /// <summary>
    /// Executes the rest request
    /// </summary>
    /// <param name="request">The rest request to execute</param>
    /// <returns>The rest response</returns>
    [StackTraceHidden]
    public IRestResponse ExecuteRequest(IRestRequest request)
    {
        _rateGate.WaitToProceed();
        return _restClient.Execute(request);
    }

    /// <summary>
    /// Returns the signed version of the provided string
    /// </summary>
    /// <param name="stringToSign">The string to sign</param>
    /// <returns>The signed string</returns>
    public string Sign(string stringToSign)
    {
        var messageBytes = Encoding.UTF8.GetBytes(stringToSign);

        var computedHash = _hmacSha256.ComputeHash(messageBytes);
        var hex = new StringBuilder(computedHash.Length * 2);
        foreach (var b in computedHash)
        {
            hex.Append($"{b:x2}");
        }

        return hex.ToString();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _hmacSha256.DisposeSafely();
        _rateGate.DisposeSafely();
    }

    private static string GetNonce()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
    }
}