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

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Time in force
/// </summary>
public enum TimeInForce
{
    /// <summary>
    /// Good till Cancel
    /// </summary>
    GTC,
    /// <summary>
    /// Immediate or Cancel
    /// </summary>
    IOC,
    /// <summary>
    /// Fill or Kill
    /// </summary>
    FOK,
    /// <summary>
    /// Post Only <seealso href="https://www.bybit.com/en-US/help-center/bybitHC_Article?language=en_US&id=000001051"/>
    /// </summary>
    PostOnly
}