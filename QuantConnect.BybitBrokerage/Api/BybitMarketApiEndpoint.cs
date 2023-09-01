using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Securities;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit market api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/market/time"/>
/// </summary>
public class BybitMarketApiEndpoint : BybitApiEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BybitMarketApiEndpoint"/> class
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitMarketApiEndpoint(ISymbolMapper symbolMapper, string apiPrefix, ISecurityProvider securityProvider,
        BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider, apiClient)
    {
    }


    /// <summary>
    /// Query for historical KLines (also known as candles/candlesticks). Charts are returned in groups based on the requested interval
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="symbol">The symbol to query the data for</param>
    /// <param name="resolution">The desired resolution</param>
    /// <param name="from">The desired start time</param>
    /// <param name="to">The end time</param>
    /// <returns>An enumerable of KLines</returns>
    public IEnumerable<ByBitKLine> GetKLines(BybitProductCategory category, string symbol, Resolution resolution,
        DateTime from, DateTime to)
    {
        var fromMs = (long)Time.DateTimeToUnixTimeStampMilliseconds(from);
        var toMs = (long)Time.DateTimeToUnixTimeStampMilliseconds(to);

        // There is no pagination support so we need to figure out the max range we can request in one batch and set the from/to times accordingly
        const int maxKLinesPerRequest = 1000;

        //max timespan we can cover with one request
        var maxTimeSpanInMs = maxKLinesPerRequest * (long)resolution.ToTimeSpan().TotalMilliseconds;
        var msToNextBar = (long)resolution.ToTimeSpan().TotalMilliseconds;

        while (fromMs < toMs)
        {
            var currentTo = fromMs + maxTimeSpanInMs;

            //Bybit returns the KLines from newest to oldest, so we need to reverse them
            var kLines = FetchKLines(category, symbol, resolution, maxKLinesPerRequest, fromMs, currentTo)
                .Reverse();

            var lastCandleOpen = fromMs;
            foreach (var kLine in kLines)
            {
                // Making sure to not return more kLines then we need in case Bybit returns more than expected
                if (kLine.OpenTime < toMs)
                {
                    lastCandleOpen = kLine.OpenTime;
                    yield return kLine;
                }
                else
                {
                    yield break;
                }
            }

            // Start time of the next request is the next candle
            fromMs = lastCandleOpen + msToNextBar;
        }
    }

    private ByBitKLine[] FetchKLines(BybitProductCategory category, string symbol, Resolution resolution, int limit,
        long? start = null, long? end = null)
    {
        var parameters = new Dictionary<string, string>
        {
            { "symbol", symbol },
            { "interval", GetIntervalString(resolution) },
            { "limit", limit.ToStringInvariant() }
        };

        if (start.HasValue)
        {
            parameters.Add("start", start.ToStringInvariant());
        }

        if (end.HasValue)
        {
            parameters.Add("end", end.ToStringInvariant());
        }

        return ExecuteGetRequest<BybitPageResult<ByBitKLine>>("/market/kline", category, parameters).List;
    }

    /// <summary>
    /// Query for the instrument specification of online trading pairs
    /// </summary>
    /// <param name="category">The product category</param>
    /// <returns>An enumerable of instrument infos</returns>
    public IEnumerable<BybitInstrumentInfo> GetInstrumentInfo(BybitProductCategory category)
    {
        return FetchAll<BybitInstrumentInfo>("/market/instruments-info", category, 1000);
    }


    /// <summary>
    /// Query for the latest price snapshot, best bid/ask price, and trading volume in the last 24 hours.
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="symbol">The symbol to query for</param>
    /// <returns>The current ticker information</returns>
    public BybitTicker GetTicker(BybitProductCategory category, string symbol)
    {
        var result = ExecuteGetRequest<BybitPageResult<BybitTicker>>("/market/tickers", category,
            new[] { new KeyValuePair<string, string>("symbol", symbol) });
        return result.List[0];
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