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
/// Order Status
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Created
    /// </summary>
    Created,

    /// <summary>
    /// New
    /// </summary>
    New,

    /// <summary>
    /// Rejected
    /// </summary>
    Rejected,

    /// <summary>
    /// Partially filled
    /// </summary>
    PartiallyFilled,

    /// <summary>
    /// Partially filled canceled
    /// </summary>
    PartiallyFilledCanceled,

    /// <summary>
    /// Filled
    /// </summary>
    Filled,

    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Untriggered
    /// </summary>
    Untriggered,

    /// <summary>
    /// Triggered
    /// </summary>
    Triggered,

    /// <summary>
    /// Deactivated
    /// </summary>
    Deactivated,

    /// <summary>
    /// Active
    /// </summary>
    Active
}