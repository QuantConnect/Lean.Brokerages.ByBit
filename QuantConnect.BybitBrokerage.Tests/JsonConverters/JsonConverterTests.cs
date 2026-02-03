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
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Brokerages.Bybit.Converters;
using QuantConnect.Brokerages.Bybit.Models;

namespace QuantConnect.Brokerages.Bybit.Tests.JsonConverters;

public class JsonConverterTests
{
    [TestCase("true", true)]
    [TestCase("1", true)]
    [TestCase("\"yes\"", true)]
    [TestCase("\"y\"", true)]
    [TestCase("\"1\"", true)]
    [TestCase("\"true\"", true)]
    [TestCase("false", false)]
    [TestCase("0", false)]
    [TestCase("\"0\"", false)]
    [TestCase("\"no\"", false)]
    [TestCase("\"n\"", false)]
    [TestCase("\"false\"", false)]
    public void BybitBoolConverterTests(string jsonValue, bool expected)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new ByBitBoolConverter()
            }
        };

        var jsonString = TestObject<bool>.CreateJsonObject(jsonValue);
        var obj = JsonConvert.DeserializeObject<TestObject<bool>>(jsonString, settings);

        Assert.AreEqual(expected, obj.Value);
    }


    [Test, TestCaseSource(nameof(CandleTimeParameters))]
    public void BybitCandleTimeConverterTests(decimal value, DateTime expected)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new BybitCandleTimeConverter()
            }
        };

        var jsonString = TestObject<bool>.CreateJsonObject($"\"{value.ToStringInvariant()}\"");
        var obj = JsonConvert.DeserializeObject<TestObject<DateTime>>(jsonString, settings);

        Assert.AreEqual(expected, obj.Value);
    }


    [TestCase("1", 1, true)]
    [TestCase("0", 0, true)]
    [TestCase("-1", -1, true)]
    [TestCase("1.333", 1.333, true)]
    [TestCase("1.333", 1.333, false)]
    [TestCase("9e-8", 0.00000009, true)]
    [TestCase("1.3117285e+06", 1311728.5, true)]
    [TestCase("9e-8", 0.00000009, false)]
    [TestCase("1.3117285e+06", 1311728.5, false)]
    public void BybitDecimalStringConverterTests(string value, decimal expected, bool quote = true)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new BybitDecimalStringConverter()
            }
        };

        var jsonString = TestObject<decimal>.CreateJsonObject($"{value}", quote);
        var obj = JsonConvert.DeserializeObject<TestObject<decimal>>(jsonString, settings);

        Assert.AreEqual(expected, obj.Value);
    }


    [Test, TestCaseSource(nameof(BybitTimeParameters))]
    public void BybitTimeConverterTests(string value, DateTime expected)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new BybitTimeConverter()
            }
        };

        var jsonString = TestObject<DateTime>.CreateJsonObject($"{value}");
        var obj = JsonConvert.DeserializeObject<TestObject<DateTime>>(jsonString, settings);

        Assert.AreEqual(expected, obj.Value);
    }

    [TestCase(new[] { "1", "100", "200", "0", "150", "1000", "10000" }, 1, 100, 200, 0, 150, 1000, 10000)]
    [TestCase(new object[] { 1, 100, 200, 0, 150, 1000, 10000 }, 1, 100, 200, 0, 150, 1000, 10000)]
    [TestCase(new[] { "1770125280000", "984590.9", "1e+06", "984590.9", "999990", "0.006", "5994.4068" }, 1770125280000,
        984590.9, 1000000.0, 984590.9, 999990, 0.006, 5994.4068)]
    public void BybitKlineJsonConverterTests(object[] data, long openTime, decimal open, decimal high, decimal low,
        decimal close,
        decimal turnover, decimal volume)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new BybitDecimalStringConverter()
            }
        };

        var stringifiedData = data.Select(o =>
            o switch
            {
                String s => "\"" + s + "\"",
                _ => o.ToString()
            }
        );
        var arrayString = '[' + string.Join(',', stringifiedData) + ']';
        var jsonString = TestObject<ByBitKLine>.CreateJsonObject(arrayString);

        var kline = JsonConvert.DeserializeObject<TestObject<ByBitKLine>>(jsonString, settings).Value;

        Assert.AreEqual(openTime, kline.OpenTime);
        Assert.AreEqual(open, kline.Open);
        Assert.AreEqual(high, kline.High);
        Assert.AreEqual(low, kline.Low);
        Assert.AreEqual(close, kline.Close);
        Assert.AreEqual(turnover, kline.Turnover);
        Assert.AreEqual(volume, kline.Volume);
    }

    [TestCase(1, 100)]
    public void BybitOrderBookRowJsonConverterTests(decimal price, decimal size)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new BybitOrderBookRowJsonConverter()
            }
        };
        var dataArray = new object[]
        {
            price,
            size
        };
        var arrayString = '[' + string.Join(',', dataArray) + ']';
        var jsonString = TestObject<BybitOrderBookRow>.CreateJsonObject(arrayString);

        var orderBookRow = JsonConvert.DeserializeObject<TestObject<BybitOrderBookRow>>(jsonString, settings).Value;

        Assert.AreEqual(price, orderBookRow.Price);
        Assert.AreEqual(size, orderBookRow.Size);
    }

    private class TestObject<T>
    {
        public T Value { get; set; }

        public static string CreateJsonObject(string value, bool quote = false)
        {
            if (quote)
            {
                value = $"\"{value}\"";
            }

            return $"{{\"value\":{value}}}";
        }
    }

    /// <summary>
    /// Provides the data required to test each order type in various cases
    /// </summary>
    private static TestCaseData[] CandleTimeParameters()
    {
        return new[]
        {
            new TestCaseData(1585180700.0647m, new DateTime(2020, 03, 25, 23, 58, 20, 65)),
            new TestCaseData(1585180700.065m,
                new DateTime(2020, 03, 25, 23, 58, 20, 65)) //It's only using ms even though bybit provides ns precision
        };
    }

    /// <summary>
    /// Provides the data required to test each order type in various cases
    /// </summary>
    private static TestCaseData[] BybitTimeParameters()
    {
        return new[]
        {
            new TestCaseData("\"1585180700064\"", new DateTime(2020, 03, 25, 23, 58, 20, 64)),
            new TestCaseData("1585180700065",
                new DateTime(2020, 03, 25, 23, 58, 20, 65)) //It's only using ms even though bybit provides ns precision
        };
    }
}