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
using System.Timers;
using QuantConnect.Brokerages;

namespace QuantConnect.BybitBrokerage;

/// <summary>
/// Wrapper class for a Bybit websocket connection
/// </summary>
public class BybitWebSocketWrapper : WebSocketClientWrapper
{
    private Timer _pingTimer;
    
    /// <summary>
    /// Event invocator for the <see cref="WebSocketClientWrapper.Open"/> event
    /// </summary>
    protected override void OnOpen()
    {
        CleanUpTimer();
        _pingTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
        _pingTimer.Elapsed += PingTimerElapsed;
        _pingTimer.Start();
        base.OnOpen();
    }

    /// <summary>
    /// Event invocator for the <see cref="WebSocketClientWrapper.Close"/> event
    /// </summary>
    protected override void OnClose(WebSocketCloseData e)
    {
        CleanUpTimer();
        base.OnClose(e);
    }

    /// <summary>
    /// Event invocator for the <see cref="WebSocketClientWrapper.OnError"/> event
    /// </summary>
    protected override void OnError(WebSocketError e)
    {
        CleanUpTimer();
        base.OnError(e);
    }

    /// <summary>
    /// Helper method to clean up timer if required
    /// </summary>
    private void CleanUpTimer()
    {
        if (_pingTimer != null)
        {
            _pingTimer.Stop();
            _pingTimer.Elapsed -= PingTimerElapsed;
            _pingTimer.Dispose();
            _pingTimer = null;
        }
    }

    private void PingTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Send("{\"op\":\"ping\"}");
    }
}