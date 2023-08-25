using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Orders;
using QuantConnect.Securities;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitMarketApiClient : BybitRestApiClient
{
    public BybitMarketApiClient(ISymbolMapper symbolMapper, string apiPrefix, IRestClient restClient, ISecurityProvider securityProvider, Action<IRestRequest> requestAuthenticator) : base(symbolMapper, apiPrefix, restClient, securityProvider, requestAuthenticator)
    {
    }
    
    public IEnumerable<ByBitKLine> GetKLines(BybitAccountCategory category,string symbol, Resolution resolution, long from, long to)
    { 
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
    public IEnumerable<ByBitKLine> GetKLines(BybitAccountCategory category,string symbol, Resolution resolution, DateTime from, DateTime to)
    {
        return GetKLines(category,symbol, resolution, (long) Time.DateTimeToUnixTimeStampMilliseconds(from),(long) Time.DateTimeToUnixTimeStampMilliseconds(to));


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

    public IEnumerable<BybitInstrumentInfo> GetInstrumentInfo(BybitAccountCategory category)
    {
        return FetchAll(category, FetchInstrumentInfo);
    }
    
    private BybitPageResult<BybitInstrumentInfo> FetchInstrumentInfo(BybitAccountCategory category,
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

    public BybitTicker GetTicker(BybitAccountCategory category, string symbol)
    {
        var endpoint = $"{ApiPrefix}/market/tickers";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("symbol", symbol);

        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitPageResult<BybitTicker>>(response).List.Single();
    }

    public BybitTicker GetTicker(BybitAccountCategory category, Order order)
    {
        var symbol = SymbolMapper.GetBrokerageSymbol(order.Symbol);
        return GetTicker(category, symbol);
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