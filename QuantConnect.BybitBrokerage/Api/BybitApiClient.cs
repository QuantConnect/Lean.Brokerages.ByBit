using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QuantConnect.Util;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitApiClient : IDisposable
{
    private readonly HMACSHA256 _hmacsha256;
    private readonly string _apiKey;
    private readonly RestClient _restClient;
    private readonly RateGate _rateGate;

    public BybitApiClient(
        string apiKey,
        string apiSecret,
        string restApiUrl)
    {
        _restClient = new RestClient(restApiUrl);
        _apiKey = apiKey;
        _hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret ?? string.Empty));
        //Todo this limit is variable and based on the vip status, how to do that in the best way? The range is 10/s -> 300/s
        _rateGate = new RateGate(10, Time.OneSecond);

    }

   




    public void Dispose()
    {
        _hmacsha256.DisposeSafely();
        _rateGate.DisposeSafely();
    }
    
    public void AuthenticateRequest(IRestRequest request)
    {
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

    public IRestResponse ExecuteRequest(IRestRequest request)
    {
        _rateGate.WaitToProceed();
        return _restClient.Execute(request);
    }

    public string Sign(string queryString)
    {
        var messageBytes = Encoding.UTF8.GetBytes(queryString);

        var computedHash = _hmacsha256.ComputeHash(messageBytes);
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