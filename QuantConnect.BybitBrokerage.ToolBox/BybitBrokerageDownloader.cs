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
using System.Linq;
using NodaTime;
using QuantConnect.Brokerages;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Securities;
using QuantConnect.Util;

namespace QuantConnect.BybitBrokerage.ToolBox
{
    /// <summary>
    /// Template Brokerage Data Downloader implementation
    /// </summary>
    public class BybitBrokerageDownloader : IDataDownloader
    {
        private readonly string _market;
        private readonly SymbolPropertiesDatabaseSymbolMapper _symbolMapper;

        public BybitBrokerageDownloader(string market = Market.Bybit)
        {
            _market = market;
            _symbolMapper = new(_market);
        }

        private Brokerage CreateBrokerage()
        {
            var apiUrl = Config.Get("bybit-api-url", "https://api.bybit.com");
            return CreateBrokerage(apiUrl);
        }

        protected virtual Brokerage CreateBrokerage(string apiUrl)
        {
            return new BybitBrokerage(string.Empty, string.Empty, apiUrl, string.Empty, null, null, null);
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

            var historyRequest = new HistoryRequest(
                startUtc,
                endUtc,
                resolution == Resolution.Tick ? typeof(Tick) : tickType == TickType.Trade ? typeof(TradeBar) : typeof(OpenInterest),
                symbol,
                resolution,
                SecurityExchangeHours.AlwaysOpen(DateTimeZone.Utc),
                DateTimeZone.Utc,
                resolution,
                false,
                false,
                DataNormalizationMode.Raw,
                tickType);

            var brokerage = CreateBrokerage();
            return brokerage.GetHistory(historyRequest);
        }

        /// <summary>
        /// Creates Lean Symbol
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        private Symbol GetSymbol(string ticker, SecurityType securityType)
        {
            return _symbolMapper.GetLeanSymbol(ticker, securityType, _market);
        }

        public static void DownloadHistory(List<string> tickers, string resolution, string securityType,
            DateTime fromDate, DateTime toDate, string tickType, string market = Market.Bybit)
        {
            if (resolution.IsNullOrEmpty() || tickers.IsNullOrEmpty())
            {
                Console.WriteLine("ByBitHistoryDownloader ERROR: '--tickers=' or '--resolution=' parameter is missing");
                Console.WriteLine("--tickers=eg BTCUSD");
                Console.WriteLine("--resolution=Minute/Hour/Daily/All");
                Console.WriteLine("optional: --security-type=Crypto/CryptoFuture");
                Console.WriteLine("optional: --tick-type=Trade,OpenInterest");
                Environment.Exit(1);
            }

            if (!Enum.TryParse<SecurityType>(securityType, true, out var securityTypeEnum))
            {
                securityTypeEnum = SecurityType.Crypto;
            }

            if (!Enum.TryParse<TickType>(tickType, true, out var tickTypeEnum))
            {
                tickTypeEnum = TickType.Trade;
            }

            try
            {
                var allResolutions = resolution.Equals("all", StringComparison.OrdinalIgnoreCase);
                var castResolution = allResolutions
                    ? tickTypeEnum == TickType.Trade ? Resolution.Minute : Resolution.Hour
                    : (Resolution)Enum.Parse(typeof(Resolution), resolution);

                //Load settings from config.json
                var dataDirectory = Config.Get("data-folder", Globals.DataFolder);

                var downloader =  CreateDownloader(securityTypeEnum,market);

                foreach (var ticker in tickers)
                {
                    // Download the data
                    var symbol = downloader.GetSymbol(ticker, securityTypeEnum);
                    var data = downloader.Get(new DataDownloaderGetParameters(symbol, castResolution, fromDate, toDate, tickType: tickTypeEnum));
                    if (data == null)
                    {
                        continue;
                    }

                    // todo how to write open interest data
                    // Save the data (single resolution)
                    var writer = new LeanDataWriter(castResolution, symbol, dataDirectory);
                    writer.Write(data);

                    if (tickTypeEnum == TickType.Trade && allResolutions && castResolution != Resolution.Tick)
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

        private static BybitBrokerageDownloader CreateDownloader(SecurityType securityType, string market)
        {
            switch (securityType)
            {
                case SecurityType.Crypto:
                case SecurityType.CryptoFuture:
                    return new BybitBrokerageDownloader(market);
                default:
                    throw new NotSupportedException(
                        $"Only {nameof(SecurityType.Crypto)} and {nameof(SecurityType.CryptoFuture)} are supported");
            }
        }
    }
}