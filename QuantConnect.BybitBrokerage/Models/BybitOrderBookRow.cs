﻿/*
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

using Newtonsoft.Json;
using QuantConnect.Brokerages.Bybit.Converters;

namespace QuantConnect.Brokerages.Bybit.Models;

/// <summary>
/// Bybit order book row
/// </summary>
[JsonConverter(typeof(BybitOrderBookRowJsonConverter))]
public class BybitOrderBookRow
{
    /// <summary>
    /// Bid or Ask price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Bid or Ask size. The delta data has size=0, which means that all quotations for this price have been filled or cancelled
    /// </summary>
    public decimal Size { get; set; }
}