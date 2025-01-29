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
using QuantConnect.Brokerages.Bybit.Api;
using QuantConnect.Brokerages.Bybit.Models;
using QuantConnect.Brokerages.Bybit.Models.Enums;
using QuantConnect.Configuration;
using QuantConnect.ToolBox;

namespace QuantConnect.Brokerages.Bybit.ToolBox
{
    /// <summary>
    /// Template Brokerage implementation of <see cref="IExchangeInfoDownloader"/>
    /// </summary>
    public class BybitExchangeInfoDownloader : IExchangeInfoDownloader
    {
        /// <summary>
        /// Market
        /// </summary>
        public string Market => QuantConnect.Market.Bybit;

        /// <summary>
        /// Get exchange info coma-separated data
        /// </summary>
        /// <returns>Enumerable of exchange info for this market</returns>
        public IEnumerable<string> Get()
        {
            var apiUrl = Config.Get("bybit-api-url", "https://api.bybit.com");
            if (apiUrl.Contains("testnet"))
            {
                throw new Exception("Testnet is not supported. Please use the production API URL.");
            }
            using var client = new BybitApi(null, null, null, null, apiUrl);

            var linear = (SecurityType: SecurityType.CryptoFuture,
                InstrumentInfos: client.Market.GetInstrumentInfo(BybitProductCategory.Linear));
            var inverse = (SecurityType: SecurityType.CryptoFuture,
                InstrumentInfos: client.Market.GetInstrumentInfo(BybitProductCategory.Inverse));
            var spot = (SecurityType: SecurityType.Crypto,
                InstrumentInfos: client.Market.GetInstrumentInfo(BybitProductCategory.Spot));

            var symbols = new[] { linear, inverse, spot }
                .SelectMany(
                    result => result.InstrumentInfos.Select(info => (result.SecurityType, InstrumentInfo: info)));

            foreach (var symbol in symbols)
            {
                if (!symbol.InstrumentInfo.Status.Equals("trading", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (string.Equals(symbol.InstrumentInfo.SettleCoin, "USDC", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Skip USDC perp and future contracts for now, they'll need some more implementation
                    continue;
                }

                if (string.Equals(symbol.InstrumentInfo.ContractType, "LinearFutures", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Skip LinearFutures contracts as they have expiration dates (USDC Futures)
                    continue;
                }

                if (string.Equals(symbol.InstrumentInfo.ContractType, "InverseFutures", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Skip crypto futures that are not perpetual
                    continue;
                }

                yield return GetInstrumentInfoString(symbol.InstrumentInfo, symbol.SecurityType);
            }
        }


        private string GetInstrumentInfoString(BybitInstrumentInfo info, SecurityType securityType)
        {
            var securityTypeStr = securityType.ToStringInvariant().ToLowerInvariant();

            // Remove multiplier prefix from symbols like 10000LADYSUSDT. Not 1 as there is also 1INCHUSDT
            var symbolName = info.Symbol.StartsWith("10") ? info.Symbol.TrimStart('0', '1') : info.Symbol;

            // market,symbol,type,description,quote_currency,contract_multiplier,minimum_price_variation,lot_size,market_ticker,minimum_order_size,price_magnifier
            return
                $"{Market.ToLowerInvariant()},{symbolName},{securityTypeStr},{info.Symbol},{info.QuoteCoin},1,{info.PriceFilter.TickSize},{info.LotSizeFilter.QuantityStep ?? info.LotSizeFilter.BasePrecision},{info.Symbol},{info.LotSizeFilter.MinOrderQuantity}";
        }
    }
}