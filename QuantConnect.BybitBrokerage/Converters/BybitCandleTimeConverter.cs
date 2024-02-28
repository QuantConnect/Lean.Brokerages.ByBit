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

namespace QuantConnect.Brokerages.Bybit.Converters;

/// <summary>
/// JSON converter to convert Bybits candle time representation
/// </summary>
public class BybitCandleTimeConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can read JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="JsonConverter"/> can read JSON; otherwise, <c>false</c>.</value>
    public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
    /// </summary>
    /// <value><c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.</value>
    public override bool CanWrite => false;

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="hasExistingValue">The existing value has a value.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var str = (string)reader.Value;
        if (str == null || !decimal.TryParse(str, out var ts))
        {
            return existingValue;
        }

        return Convert(ts);
    }


    /// <summary>
    /// Converts Bybits candle time representation to <see cref="DateTime"/>
    /// </summary>
    /// <param name="ts">The timestamp in Bybits candle time representation</param>
    /// <returns>The <see cref="DateTime"/> representing the input timestamp</returns>
    public static DateTime Convert(decimal ts)
    {
        var seconds = (long)ts;
        var ms = Math.Round(ts % 1M, 3) * 1000M;
        ms += seconds * 1000;

        var dto = DateTimeOffset.FromUnixTimeMilliseconds((long)ms);
        return dto.DateTime;
    }
}