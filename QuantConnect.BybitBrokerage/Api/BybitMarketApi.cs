using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit market api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/market/time"/>
/// </summary>
public class BybitMarketApi : BybitBaseApi
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitMarketApi"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitMarketApi(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider, BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
    {
    }
    
    /// <summary>
    /// Query for historical klines (also known as candles/candlesticks). Charts are returned in groups based on the requested interval
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="symbol">The symbol to query the data for</param>
    /// <param name="resolution">The desired resolution</param>
    /// <param name="from">The desired start time in ms</param>
    /// <param name="to">The end time in ms</param>
    /// <returns>An enumerable of KLines</returns>
    public IEnumerable<ByBitKLine> GetKLines(BybitProductCategory category,string symbol, Resolution resolution, long from, long to)
    { 
        
        //todo explain the magic
        var msToNextBar = (long) resolution.ToTimeSpan().TotalMilliseconds;
        var maxTimeSpan = 199 * (long)resolution.ToTimeSpan().TotalMilliseconds;
        while (from < to)
        {
            var curTo = from + maxTimeSpan;
            var response = FetchKLines(category, symbol, resolution, from, curTo).Reverse().ToArray();
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
    
    /// <summary>
    /// Query for historical klines (also known as candles/candlesticks). Charts are returned in groups based on the requested interval
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="symbol">The symbol to query the data for</param>
    /// <param name="resolution">The desired resolution</param>
    /// <param name="from">The desired start time</param>
    /// <param name="to">The end time</param>
    /// <returns>An enumerable of KLines</returns>
    public IEnumerable<ByBitKLine> GetKLines(BybitProductCategory category,string symbol, Resolution resolution, DateTime from, DateTime to)
    {
        return GetKLines(category,symbol, resolution, (long) Time.DateTimeToUnixTimeStampMilliseconds(from),(long) Time.DateTimeToUnixTimeStampMilliseconds(to));
    }
    
    private ByBitKLine[] FetchKLines(BybitProductCategory category, string symbol,Resolution resolution,  long? start = null, long? end = null)
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

    /// <summary>
    /// Query for the instrument specification of online trading pairs
    /// </summary>
    /// <param name="category">The product category</param>
    /// <returns>An enumerable of instrument infos</returns>
    public IEnumerable<BybitInstrumentInfo> GetInstrumentInfo(BybitProductCategory category)
    {
        return FetchAll(category, FetchInstrumentInfo);
    }
    
    private BybitPageResult<BybitInstrumentInfo> FetchInstrumentInfo(BybitProductCategory category,
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

    /// <summary>
    /// Query for the latest price snapshot, best bid/ask price, and trading volume in the last 24 hours.
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="symbol">The symbol to query for</param>
    /// <returns>The current ticker information</returns>
    public BybitTicker GetTicker(BybitProductCategory category, string symbol)
    {
        var endpoint = $"{ApiPrefix}/market/tickers";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("symbol", symbol);

        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitPageResult<BybitTicker>>(response).List.Single();
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


}