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
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Securities;
using OrderStatus = QuantConnect.Orders.OrderStatus;
using OrderType = QuantConnect.BybitBrokerage.Models.Enums.OrderType;

namespace QuantConnect.BybitBrokerage;

public partial class BybitBrokerage
{
    #region Brokerage

    /// <summary>
    /// Gets all open orders on the account.
    /// NOTE: The order objects returned do not have QC order IDs.
    /// </summary>
    /// <returns>The open orders returned from Bybit</returns>
    public override List<Order> GetOpenOrders()
    {
        var orders = new List<Order>();
        foreach (var category in SupportedBybitProductCategories)
        {
            orders.AddRange(ApiClient.Trade.GetOpenOrders(category)
                .Select(bybitOrder =>
                {
                    var symbol = _symbolMapper.GetLeanSymbol(bybitOrder.Symbol, GetSecurityType(category), MarketName);
                    var price = bybitOrder.Price!.Value;
                    Order order;
                    if (bybitOrder.StopOrderType != null)
                    {
                        if (bybitOrder.StopOrderType == StopOrderType.TrailingStop)
                        {
                            throw new NotSupportedException();
                        }

                        order = bybitOrder.OrderType == OrderType.Limit
                            ? new StopLimitOrder(symbol, bybitOrder.Quantity, price, bybitOrder.Price!.Value, bybitOrder.CreateTime)
                            : new StopMarketOrder(symbol, bybitOrder.Quantity, price, bybitOrder.CreateTime);
                    }
                    else
                    {
                        order = bybitOrder.OrderType == OrderType.Limit
                            ? new LimitOrder(symbol, bybitOrder.Quantity, price, bybitOrder.CreateTime)
                            : new MarketOrder(symbol, bybitOrder.Quantity, bybitOrder.CreateTime);
                    }

                    order.BrokerId.Add(bybitOrder.OrderId);
                    order.Status = ConvertOrderStatus(bybitOrder.Status);
                    return order;
                }));
        }

        return orders;
    }

    /// <summary>
    /// Gets all holdings for the account
    /// </summary>
    /// <returns>The current holdings from the account</returns>
    public override List<Holding> GetAccountHoldings()
    {
        var holdings = new List<Holding>();
        foreach (var category in SupportedBybitProductCategories)
        {
            holdings.AddRange(ApiClient.Position.GetPositions(category)
                .Select(bybitPosition => new Holding
                {
                    Symbol = _symbolMapper.GetLeanSymbol(bybitPosition.Symbol, GetSecurityType(category), MarketName),
                    AveragePrice = bybitPosition.AveragePrice,
                    Quantity = bybitPosition.Side == Models.Enums.PositionSide.Buy ? bybitPosition.Size : bybitPosition.Size * -1,
                    MarketValue = bybitPosition.PositionValue,
                    UnrealizedPnL = bybitPosition.UnrealisedPnl,
                    MarketPrice = bybitPosition.MarkPrice
                }));
        }

        return holdings.Count > 0 ? holdings : base.GetAccountHoldings(_job?.BrokerageData, _algorithm?.Securities?.Values);
    }

    /// <summary>
    /// Gets the current cash balance for each currency held in the brokerage account
    /// </summary>
    /// <returns>The current cash balance for each currency available for trading</returns>
    public override List<CashAmount> GetCashBalance()
    {
        return ApiClient.Account
            .GetWalletBalances().Assets
            .Select(x => new CashAmount(x.WalletBalance, x.Asset)).ToList();
    }

    /// <summary>
    /// Places a new order and assigns a new broker ID to the order
    /// </summary>
    /// <param name="order">The order to be placed</param>
    /// <returns>True if the request for a new order has been placed, false otherwise</returns>
    public override bool PlaceOrder(Order order)
    {
        if (!CanSubscribe(order.Symbol))
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1,
                $"Symbol is not supported {order.Symbol}"));
            return false;
        }

        var submitted = false;

        _messageHandler.WithLockedStream(() =>
        {
            var result = ApiClient.Trade.PlaceOrder(GetBybitProductCategory(order.Symbol), order);
            order.BrokerId.Add(result.OrderId);
            OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, OrderFee.Zero, "Bybit Order Event")
            {
                Status = OrderStatus.Submitted
            });
            submitted = true;
        });

        return submitted;
    }

    /// <summary>
    /// Updates the order with the same id
    /// </summary>
    /// <param name="order">The new order information</param>
    /// <returns>True if the request was made for the order to be updated, false otherwise</returns>
    public override bool UpdateOrder(Order order)
    {
        var category = GetBybitProductCategory(order.Symbol);
        if (category == BybitProductCategory.Spot)
        {
            throw new NotSupportedException("BybitBrokerage.UpdateOrder: Order update not supported for spot. Please cancel and re-create.");
        }

        if (!CanSubscribe(order.Symbol))
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1,
                $"Symbol is not supported {order.Symbol}"));
            return false;
        }

        var submitted = false;

        _messageHandler.WithLockedStream(() =>
        {
            ApiClient.Trade.UpdateOrder(category, order);
            OnOrderEvent(new OrderEvent(order, DateTime.UtcNow, OrderFee.Zero, "Bybit Order Event")
            {
                Status = OrderStatus.UpdateSubmitted
            });
            submitted = true;
            //OnOrderIdChangedEvent(new BrokerageOrderIdChangedEvent(){BrokerId = cachedOrder.BrokerId, OrderId = order.Id});
        });

        return submitted;
    }

    /// <summary>
    /// Cancels the order with the specified ID
    /// </summary>
    /// <param name="order">The order to cancel</param>
    /// <returns>True if the request was made for the order to be canceled, false otherwise</returns>
    public override bool CancelOrder(Order order)
    {
        if (!CanSubscribe(order.Symbol))
        {
            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1,
                $"Symbol is not supported {order.Symbol}"));
            return false;
        }

        var canceled = false;
        _messageHandler.WithLockedStream(() =>
        {
            if (order.Status == OrderStatus.Filled || order.Type == Orders.OrderType.Market)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1, "Order already filled"));
                return;
            }

            if (order.Status is OrderStatus.Canceled or OrderStatus.CancelPending)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1,
                    "Order already canceled or cancellation submitted"));
                return;
            }

            ApiClient.Trade.CancelOrder(GetBybitProductCategory(order.Symbol), order);
            canceled = true;
        });
        return canceled;
    }

    /// <summary>
    /// Wss message handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected override void OnMessage(object sender, WebSocketMessage e)
    {
        _messageHandler.HandleNewMessage(e);
    }

    /// <summary>
    /// Connects the client to the broker's remote servers
    /// </summary>
    public override void Connect()
    {
        if (IsConnected)
            return;

        // cannot reach this code if rest api client is not created
        // WebSocket is  responsible for Bybit UserData stream only
        // as a result we don't need to connect user data stream if BybitBrokerage is used as DQH only
        // or until Algorithm is actually initialized

        Connect(null);
    }

    /// <summary>
    /// Disconnects the client from the broker's remote servers
    /// </summary>
    public override void Disconnect()
    {
        if (WebSocket?.IsOpen != true) return;

        WebSocket.Close();
    }



    #endregion
}