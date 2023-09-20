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
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage.Models
{
    /// <summary>
    /// Bybits default http response message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ByBitResponse<T>
    {
        /// <summary>
        /// Success/Error code
        /// </summary>
        [JsonProperty("retCode")]
        public int ReturnCode { get; set; }

        /// <summary>
        /// Success/Error msg. OK, success, SUCCESS indicate a successful response
        /// </summary>
        [JsonProperty("retMsg")]
        public string ReturnMessage { get; set; }


        /// <summary>
        /// Extend info. Most of the time, it is <c>{}</c>
        /// </summary>
        [JsonProperty("retExtInfo")]
        public object ExtendedInfo { get; set; }

        /// <summary>
        /// Business data result
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Current time
        /// </summary>
        [JsonConverter(typeof(BybitTimeConverter))]
        public DateTime Time { get; set; }
    }
}