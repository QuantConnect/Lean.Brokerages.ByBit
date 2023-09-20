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

namespace QuantConnect.BybitBrokerage.Models.Messages;

/// <summary>
/// Base websocket data message
/// </summary>
/// <typeparam name="T">The type of the business data</typeparam>
public class BybitDataMessage<T>
{
    /// <summary>
    /// The websocket topic this message belongs to
    /// </summary>
    public string Topic { get; set; }
    
    /// <summary>
    /// The message type 
    /// </summary>
    public BybitMessageType Type { get; set; }

    /// <summary>
    /// Message Time
    /// </summary>
    [JsonProperty("ts")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime Time { get; set; }

    /// <summary>
    /// Business data
    /// </summary>
    public T Data { get; set; }
}