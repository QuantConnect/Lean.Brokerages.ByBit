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
using QuantConnect.ToolBox;
using System.Collections.Generic;
using System.Linq;
using QLNet;
using QuantConnect.BybitBrokerage.Api;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Configuration;

namespace QuantConnect.TemplateBrokerage.ToolBox
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
            using var client = new BybitApi(null, null, null, apiUrl);

            var linear = (SecurityType.CryptoFuture, client.Market.GetInstrumentInfo(BybitAccountCategory.Linear));
            var inverse = (SecurityType.CryptoFuture, client.Market.GetInstrumentInfo(BybitAccountCategory.Inverse));
            var spot = (SecurityType.Crypto, client.Market.GetInstrumentInfo(BybitAccountCategory.Spot));

            foreach (var symbolSource in new[] { linear, inverse, spot })
            {
                foreach (var symbol in symbolSource.Item2)
                {
                    //if (!symbol.UnifiedMarginTrade) continue;
                    if (!symbol.Status.Equals("trading", StringComparison.InvariantCultureIgnoreCase)) continue;
                    yield return GetInstrumentInfoString(symbol, symbolSource.Item1);
                }
            }
        }


        private string GetInstrumentInfoString(BybitInstrumentInfo info, SecurityType securityType)
        {
            var securityTypeStr = securityType.ToStringInvariant().ToLowerInvariant();
            //market,symbol,type,description,quote_currency,contract_multiplier,minimum_price_variation,lot_size,market_ticker,minimum_order_size,price_magnifier
            //todo what's price scale and magnifier?
            return
                $"{Market.ToLowerInvariant()},{info.Symbol},{securityTypeStr},{info.Symbol},{info.QuoteCoin},1,{info.PriceFilter.TickSize},{info.LotSizeFilter.QuantityStep ?? info.LotSizeFilter.BasePrecision},{info.Symbol},{info.LotSizeFilter.MinOrderQuantity}";
        }
    }
}