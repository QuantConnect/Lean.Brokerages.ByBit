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
using QuantConnect.Data;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using QuantConnect.Brokerages;
using QuantConnect.Configuration;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Securities;
using QuantConnect.Statistics;
using QuantConnect.Util;

namespace QuantConnect.TemplateBrokerage.ToolBox
{
    /// <summary>
    /// Template Brokerage Data Downloader implementation
    /// </summary>
    public class BybitBrokerageDownloader : IDataDownloader
    {
        private readonly string _market;
        private readonly SymbolPropertiesDatabaseSymbolMapper _symbolMapper;
        private readonly BybitBrokerage.BybitBrokerage _brokerage;

        public BybitBrokerageDownloader(string market = Market.Bybit)
        {
            _market = market;
            _symbolMapper = new(_market);

            var apiUrl = Config.Get("bybit-api-url", "https://api.bybit.com");
            _brokerage = new BybitBrokerage.BybitBrokerage(string.Empty, String.Empty, apiUrl, String.Empty, null, null, null);
        }

        
        /// <summary>
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        /// </summary>
        /// <param name="dataDownloaderGetParameters">model class for passing in parameters for historical data</param>
        /// <returns>Enumerable of base data for this symbol</returns>
        public IEnumerable<BaseData> Get(DataDownloaderGetParameters dataDownloaderGetParameters)
        {
            var symbol = dataDownloaderGetParameters.Symbol;
            var resolution = dataDownloaderGetParameters.Resolution;
            var startUtc = dataDownloaderGetParameters.StartUtc;
            var endUtc = dataDownloaderGetParameters.EndUtc;
            var tickType = dataDownloaderGetParameters.TickType;
            
            
            if (tickType != TickType.Trade)
            {  return Enumerable.Empty<BaseData>();
            }
            

            if (!_symbolMapper.IsKnownLeanSymbol(symbol))
                throw new ArgumentException($"The ticker {symbol.Value} is not available.");

            if (endUtc < startUtc)
                throw new ArgumentException("The end date must be greater or equal than the start date.");

            var historyRequest = new HistoryRequest(
                startUtc,
                endUtc,
                resolution == Resolution.Tick ? typeof(Tick) : typeof(TradeBar),
                symbol,
                resolution,
                SecurityExchangeHours.AlwaysOpen(DateTimeZone.Utc), 
                DateTimeZone.Utc,
                resolution,
                false,
                false,
                DataNormalizationMode.Raw,
                tickType);

            var data = _brokerage.GetHistory(historyRequest);
            return data;

        }
        /// <summary>
        /// Creates Lean Symbol
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        private Symbol GetSymbol(string ticker)
        {
            return _symbolMapper.GetLeanSymbol(ticker, SecurityType.CryptoFuture, _market);
        }

          public static void DownloadHistory(List<string> tickers, string resolution, string securityType,
            DateTime fromDate, DateTime toDate, string market = Market.Bybit)
        {
            if (resolution.IsNullOrEmpty() || tickers.IsNullOrEmpty())
            {
                Console.WriteLine("ByBitHistoryDownloader ERROR: '--tickers=' or '--resolution=' parameter is missing");
                Console.WriteLine("--tickers=eg BTCUSD");
                Console.WriteLine("--resolution=Minute/Hour/Daily/All");
                Environment.Exit(1);
            }

            try
            {
                var allResolutions = resolution.Equals("all", StringComparison.OrdinalIgnoreCase);
                var castResolution = allResolutions
                    ? Resolution.Minute
                    : (Resolution)Enum.Parse(typeof(Resolution), resolution);

                // Load settings from config.json
                var dataDirectory = Config.Get("data-folder", Globals.DataFolder);

                var downloader = new BybitBrokerageDownloader(market);

                foreach (var ticker in tickers)
                {
                    // Download the data
                    var symbol = downloader.GetSymbol(ticker);
                    var data = downloader.Get(new DataDownloaderGetParameters(symbol, castResolution, fromDate,
                        toDate, tickType: TickType.Trade));
                
                    //var bars = data.Cast<TradeBar>().ToList();

                    
                    // Save the data (single resolution)
                    var writer = new LeanDataWriter(castResolution, symbol, dataDirectory);
                    writer.Write(data);

                    
                    if (allResolutions && castResolution != Resolution.Tick)
                    {
                        var bars = data.Cast<TradeBar>();
                        // Save the data (other resolutions)
                        foreach (var res in new[] { Resolution.Hour, Resolution.Daily })
                        {
                            var resData = LeanData.AggregateTradeBars(bars, symbol, res.ToTimeSpan());

                            writer = new LeanDataWriter(res, symbol, dataDirectory);
                            writer.Write(resData);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Log.Error(err);
            }
        }
    }
}