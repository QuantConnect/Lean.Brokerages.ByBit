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

/// <summary>
/// Base Bybit api endpoint implementation
/// </summary>
public abstract class BybitBaseApi
{
    /// <summary>
    /// Default api JSON serializer settings
    /// </summary>
    protected static readonly JsonSerializerSettings SerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Converters = new List<JsonConverter>(){new ByBitKlineJsonConverter(), new StringEnumConverter(), new BybitDecimalStringConverter()},
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly BybitApiClient _apiClient;

    /// <summary>
    /// Api prefix
    /// </summary>
    protected string ApiPrefix { get; }
    
    /// <summary>
    /// Symbol mapper
    /// </summary>
    protected ISymbolMapper SymbolMapper { get; }
    
    /// <summary>
    /// Security provider
    /// </summary>
    protected ISecurityProvider SecurityProvider { get; }
    
    /// <summary>
    /// Bybit api client
    /// </summary>


    /// <summary>
    /// Initializes a new instance of the <see cref="BybitBaseApi"/> class.
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The api client</param>
    protected BybitBaseApi(
        ISymbolMapper symbolMapper,
        string apiPrefix,
        ISecurityProvider securityProvider,
        BybitApiClient apiClient)
    {
        SymbolMapper = symbolMapper;
        SecurityProvider = securityProvider;
        ApiPrefix = apiPrefix;
        _apiClient = apiClient;
    }


//todo get rid of func
    protected T[] FetchAll<T>(BybitProductCategory category, Func<BybitProductCategory, string, BybitPageResult<T>> fetch, Predicate<BybitPageResult<T>> @break  = null)
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
    
    /// <summary>
    /// Ensures the request executed successfully and returns the parsed business object
    /// </summary>
    /// <param name="response">The response to parse</param>
    /// <typeparam name="T">The type of the response business object</typeparam>
    /// <returns>The parsed response business object</returns>
    /// <exception cref="Exception"></exception>
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
                                $"Content: {response.Content}, ErrorCode: {byBitResponse?.ReturnCode} ErrorMessage: {byBitResponse?.ReturnMessage}");
        }

        if (Log.DebuggingEnabled)
        {
            Log.Debug(
                $"Bybit request for {response.Request.Resource} executed successfully. Response: {response.Content}");
        }

        return byBitResponse.Result;
    }
    
    /// <summary>
    /// Authenticates the provided request by signing it and adding the required headers
    /// </summary>
    /// <param name="request">The request to authenticate</param>
    /// <exception cref="NotSupportedException">Is thrown when an unsupported request type is provided. Only GET and POST are supported</exception>
    protected void AuthenticateRequest(IRestRequest request)
    {
        _apiClient.AuthenticateRequest(request);
    }

    /// <summary>
    /// Executes the rest request
    /// </summary>
    /// <param name="request">The rest request to execute</param>
    /// <returns>The rest response</returns>
    protected IRestResponse ExecuteRequest(IRestRequest request)
    {
        return _apiClient.ExecuteRequest(request);
    }


    

    
}