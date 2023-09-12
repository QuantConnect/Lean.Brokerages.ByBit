/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.BybitBrokerage.Models.Messages;
using QuantConnect.Logging;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Implements functionality to download historical tick data from Bybit
/// </summary>
public class BybitHistoryApi
{
    private const string BaseAddress = "https://public.bybit.com";

    private readonly IRestClient _restClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BybitHistoryApi"/> class
    /// </summary>
    public BybitHistoryApi()
    {
        _restClient = new RestClient(BaseAddress);
    }

    /// <summary>
    /// Downloads historical tick data from Bybit
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="ticker">The ticker to fetch the history for</param>
    /// <param name="from">The start time</param>
    /// <param name="to">The end time</param>
    /// <returns>An IEnumerable containing the ticks in the requested range</returns>
    public IEnumerable<BybitTickUpdate> Download(BybitProductCategory category, string ticker, DateTime from,
        DateTime to)
    {
        for (var i = from.Date; i <= to.Date; i = i.AddDays(1))
        {
            var res = Download(ticker, category, i);
            foreach (var tick in res)
            {
                if (tick.Time < from) continue;
                if (tick.Time > to) yield break;
                yield return tick;
            }
        }
    }
    
    /// <summary>
    /// Downloads historical tick data from Bybit for the specified date
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="ticker">The ticker to fetch the history for</param>
    /// <param name="date">The requested date</param>
    /// <returns>An IEnumerable containing the ticks from the requested date</returns>
    private IEnumerable<BybitTickUpdate> Download(string ticker, BybitProductCategory category, DateTime date)
    {
        if (category is not (BybitProductCategory.Inverse or BybitProductCategory.Linear or BybitProductCategory.Spot))
        {
            throw new NotSupportedException("Only inverse, linear, and spot supported");
        }

        var categoryPath = category == BybitProductCategory.Spot ? "spot" : "trading";
        var dateSeparator = category == BybitProductCategory.Spot ? "_" : string.Empty;
        var endpoint = $"/{categoryPath}/{ticker}/{ticker}{dateSeparator}{date:yyyy-MM-dd}.csv.gz";


        using var memoryStream = new MemoryStream();
        DownloadArchiveToStream(endpoint, memoryStream);
        memoryStream.Position = 0;

        using var gzip = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var streamReader = new StreamReader(gzip);

        streamReader.ReadLine(); //header
        var line = streamReader.ReadLine();
        while (!string.IsNullOrEmpty(line))
        {
            if (category == BybitProductCategory.Spot)
            {
                yield return ParseSpotTick(line, ticker);
            }
            else if (category is BybitProductCategory.Inverse or BybitProductCategory.Linear)
            {
                var tick = ParseFuturesTick(line);
                if (tick != null) yield return tick;
            }
            else
            {
                throw new NotSupportedException();
            }

            line = streamReader.ReadLine();
        }
    }

    private void DownloadArchiveToStream(string endpoint, Stream stream)
    {
        var req = new RestRequest(endpoint)
        {
            ResponseWriter = responseStream => responseStream.CopyTo(stream)
        };

        var response = _restClient.Execute(req);

        if (!response.IsSuccessful)
        {
            throw new Exception("ByBitApiClient request failed: " +
                                $"[{(int)response.StatusCode}] {response.StatusDescription}, " +
                                $"Content: {response.Content}, ErrorMessage: {response.ErrorMessage}");
        }
    }

    private static BybitTickUpdate ParseSpotTick(string line, string ticker)
    {
        var tick = new BybitTickUpdate();
        var split = line.Split(',');
        tick.Time = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(split[1])).UtcDateTime;
        tick.Price = decimal.Parse(split[2], CultureInfo.InvariantCulture);
        tick.Value = decimal.Parse(split[3], CultureInfo.InvariantCulture);
        tick.Side = Enum.Parse<OrderSide>(split[4], true);
        tick.Symbol = ticker;
        return tick;
    }

    private static BybitTickUpdate ParseFuturesTick(string line)
    {
        var tick = new BybitTickUpdate();
        try
        {
            var split = line.Split(',');
            tick.Time = BybitCandleTimeConverter.Convert(decimal.Parse(split[0], CultureInfo.InvariantCulture));
            tick.Symbol = split[1];
            tick.Side = (OrderSide)Enum.Parse(typeof(OrderSide), split[2]);
            tick.Value = decimal.Parse(split[3], NumberStyles.Float, CultureInfo.InvariantCulture);
            tick.Price = decimal.Parse(split[4], CultureInfo.InvariantCulture);
            tick.TickType = (TickDirection)Enum.Parse(typeof(TickDirection), split[5]);
            tick.Id = split[6];
            // tick.GrossValue = decimal.Parse(split[7], CultureInfo.InvariantCulture);
            // tick.HomeNotional = decimal.Parse(split[8], CultureInfo.InvariantCulture);
            // tick.ForeignNotional = decimal.Parse(split[9], CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Log.Error(e, $"Error while parsing tick line: '{line}'");
            return null;
        }

        return tick;
    }
}