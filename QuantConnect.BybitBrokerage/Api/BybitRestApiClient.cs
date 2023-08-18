using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Logging;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitRestApiClient : IDisposable
{
    protected static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Converters = new List<JsonConverter>(){new ByBitKlineJsonConverter()},
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly ISymbolMapper _symbolMapper;
    private readonly ISecurityProvider _securityProvider;
    private readonly RestClient _restClient;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly HMACSHA256 _hmacsha256;

    protected string ApiPrefix { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="BybitRestApiClient"/> class.
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper.</param>
    /// <param name="securityProvider">The holdings provider.</param>
    /// <param name="apiKey">The Binance API key</param>
    /// <param name="apiSecret">The The Binance API secret</param>
    /// <param name="restApiUrl">The Binance API rest url</param>
    public BybitRestApiClient(
        ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl)
    {
        _symbolMapper = symbolMapper;
        _securityProvider = securityProvider;
        _restClient = new RestClient(restApiUrl);
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret ?? string.Empty));

        ApiPrefix = "/v5";
    }


    public IEnumerable<BybitOrder> GetOpenOrders(BybitAccountCategory category)
    {
        return FetchAll(category, FetchOpenOrders);
    }
    
    public IEnumerable<BybitInstrumentInfo> GetInstrumentInfo(BybitAccountCategory category)
    {
        return FetchAll(category, FetchInstruementInfo);
    }

    public BybitBalance GetWalletBalances(BybitAccountCategory category)
    {
        var endpoint = $"{ApiPrefix}/account/wallet-balance";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("accountType", category == BybitAccountCategory.Inverse ? "CONTRACT":"UNIFIED");

      
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var balance =  EnsureSuccessAndParse<BybitBalance>(response);
        return balance;
    }

    public BybitCoinBalances GetCoinBalances()
    {
        
        var endpoint = $"{ApiPrefix}/asset/transfer/query-account-coins-balance";

        var request = new RestRequest(endpoint);
        request.AddQueryParameter("accountType", "UNIFIED");

      
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var balances =  EnsureSuccessAndParse<BybitCoinBalances>(response);
        return balances;
    }

    private T[] FetchAll<T>(BybitAccountCategory category, Func<BybitAccountCategory, string, BybitPageResult<T>> fetch)
    {
        var results = new List<T>();
        string cursor = null;
        do
        {
            var result = fetch(category, cursor);
            results.AddRange(result.List);
            cursor = result.NextPageCursor;
        } while (!string.IsNullOrEmpty(cursor));

        return results.ToArray();
    }

    private BybitPageResult<BybitInstrumentInfo> FetchInstruementInfo(BybitAccountCategory category,
        string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/market/instruments-info";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("limit", "1000");

        if (cursor != null)
        {
            request.AddQueryParameter("cursor", cursor);
        }

        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitPageResult<BybitInstrumentInfo>>(response);
    }

    private ByBitKLine[] FetchKLines(BybitAccountCategory category, string symbol,Resolution resolution,  long? start = null, long? end = null)
    {
        var endpoint = $"{ApiPrefix}/market/kline";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("symbol", symbol);
        request.AddQueryParameter("interval", GetIntervalString(resolution));
        request.AddQueryParameter("limit", "200");
        if (start.HasValue)
        {
            request.AddQueryParameter("start", start.ToStringInvariant());
        }

        if (end.HasValue)
        {
            request.AddQueryParameter("end", end.ToStringInvariant());
        }

        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitPageResult<ByBitKLine>>(response).List;
    }


    public IEnumerable<ByBitKLine> GetKLines(BybitAccountCategory category,string symbol, Resolution resolution, long from, long to)
    { 
        var msToNextBar = (long) resolution.ToTimeSpan().TotalMilliseconds;
        while (from < to)
        {
            var response = FetchKLines(category, symbol, resolution, from, to).Reverse().ToArray();
            if(response.Length == 0) yield break;
            foreach (var kLine in response)
            {
                if (kLine.OpenTime < to)
                {
                    yield return kLine;
                }
                else
                {
                    yield break;
                }
            }
            from = response.Last().OpenTime + msToNextBar;
        }
    }
    public IEnumerable<ByBitKLine> GetKLines(BybitAccountCategory category,string symbol, Resolution resolution, DateTime from, DateTime to)
    {
        return GetKLines(category,symbol, resolution, (long) Time.DateTimeToUnixTimeStampMilliseconds(from),(long) Time.DateTimeToUnixTimeStampMilliseconds(to));


    }
    private static string GetIntervalString(Resolution resolution)
    {
        return resolution switch
        {
            Resolution.Daily => "D",
            Resolution.Hour => "60",
            Resolution.Minute => "1",
            Resolution.Second => throw new NotSupportedException("Smallest supported timeframe is 1 minute"),
            Resolution.Tick => throw new NotSupportedException("Smallest supported timeframe is 1 minute"),
            _ => throw new ArgumentOutOfRangeException(nameof(resolution))
        };
    }
    private BybitPageResult<BybitOrder> FetchOpenOrders(BybitAccountCategory category, string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/order/realtime";
        var request = new RestRequest(endpoint);

        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("limit", "50");
        if (cursor != null)
        {
            request.AddQueryParameter("cursor", cursor);
        }

        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var orders = EnsureSuccessAndParse<BybitPageResult<BybitOrder>>(response);
        return orders;
    }

    private T EnsureSuccessAndParse<T>(IRestResponse response)
    {
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("ByBitApiClient request failed: " +
                                $"[{(int)response.StatusCode}] {response.StatusDescription}, " +
                                $"Content: {response.Content}, ErrorMessage: {response.ErrorMessage}");
        }

        ByBitResponse<T> byBitResponse;
        try
        {
            byBitResponse = JsonConvert.DeserializeObject<ByBitResponse<T>>(response.Content, SerializerSettings);
        }
        catch (Exception e)
        {
            throw new Exception("ByBitApiClient failed deserializing response: " +
                                $"[{(int)response.StatusCode}] {response.StatusDescription}, " +
                                $"Content: {response.Content}, ErrorMessage: {response.ErrorMessage}", e);
        }

        if (byBitResponse?.ReturnCode != 0)
        {
            throw new Exception("ByBitApiClient request failed: " +
                                $"[{(int)response.StatusCode}] {response.StatusDescription}, " +
                                $"Content: {response.Content}, ErrorCode: {byBitResponse.ReturnCode} ErrorMessage: {byBitResponse.ReturnMessage}");
        }

        if (Log.DebuggingEnabled)
        {
            Log.Debug(
                $"Bybit request for {response.Request.Resource} executed successfully. Response: {response.Content}");
        }

        return byBitResponse.Result;
    }

    private IRestResponse ExecuteRequest(IRestRequest request)
    {
        //todo rate limit
        return _restClient.Execute(request);
    }

    private void AuthenticateRequest(IRestRequest request)
    {
        var parameters = request.Parameters
            .Where(x => x.Type is ParameterType.QueryStringWithoutEncode or ParameterType.QueryString)
            .OrderBy(x => x.Name)
            .Select(x => $"{x.Name}={x.Value}");

        var queryString = string.Join("&", parameters);

        var nonce = GetNonce();

        request.AddHeader("X-BAPI-SIGN", SignQueryString(queryString));
        request.AddHeader("X-BAPI-API-KEY", _apiKey);
        request.AddHeader("X-BABI-TIMESTAMP", nonce);
    }

    private string SignQueryString(string queryString)
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

    public void Dispose()
    {
    }
}