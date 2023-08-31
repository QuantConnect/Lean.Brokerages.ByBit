using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Logging;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public abstract class BybitBaseApi
{
    protected static readonly JsonSerializerSettings SerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Converters = new List<JsonConverter>(){new ByBitKlineJsonConverter(), new StringEnumConverter(), new BybitDecimalStringConverter()},
        NullValueHandling = NullValueHandling.Ignore
    };

    
    protected string ApiPrefix { get; }
    protected ISymbolMapper SymbolMapper { get; }
    protected ISecurityProvider SecurityProvider { get; }
    
    protected BybitApiClient ApiClient { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="BybitBaseApi"/> class.
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper.</param>
    /// <param name="apiPrefix"></param>
    /// <param name="securityProvider"></param>
    /// <param name="apiClient"></param>
    /// <param name="apiKey">The Binance API key</param>
    /// <param name="restApiUrl">The Bina
    /// nce API rest url</param>
    protected BybitBaseApi(
        ISymbolMapper symbolMapper,
        string apiPrefix,
        ISecurityProvider securityProvider,
        BybitApiClient apiClient)
    {
        SymbolMapper = symbolMapper;
        SecurityProvider = securityProvider;
        ApiPrefix = apiPrefix;
        ApiClient = apiClient;
    }



    protected T[] FetchAll<T>(BybitAccountCategory category, Func<BybitAccountCategory, string, BybitPageResult<T>> fetch, Predicate<BybitPageResult<T>> @break  = null)
    {
        var results = new List<T>();
        string cursor = null;
        do
        {
            var result = fetch(category, cursor);
            results.AddRange(result.List);
            cursor = result.NextPageCursor;
            if(@break?.Invoke(result) ?? false) break;
        } while (!string.IsNullOrEmpty(cursor));

        return results.ToArray();
    }
    
    [StackTraceHidden]
    protected T EnsureSuccessAndParse<T>(IRestResponse response)
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
    

    protected void AuthenticateRequest(IRestRequest request)
    {
        ApiClient.AuthenticateRequest(request);
    }

    protected IRestResponse ExecuteRequest(IRestRequest request)
    {
        return ApiClient.ExecuteRequest(request);
    }

    protected virtual BybitAccountType BybitAccountType(BybitAccountCategory category) =>
        Models.Enums.BybitAccountType.Unified;
    

    
}