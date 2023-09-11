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
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.BybitBrokerage.Models.Requests;
using QuantConnect.Orders;
using QuantConnect.Securities;
using OrderType = QuantConnect.BybitBrokerage.Models.Enums.OrderType;
using TimeInForce = QuantConnect.BybitBrokerage.Models.Enums.TimeInForce;

namespace QuantConnect.BybitBrokerage.Api;

/// <summary>
/// Bybit trade api endpoint implementation
/// <seealso href="https://bybit-exchange.github.io/docs/v5/order/create-order"/>
/// </summary>
public class BybitTradeApiEndpoint : BybitApiEndpoint
{
    private readonly BybitMarketApiEndpoint _marketApiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BybitTradeApiEndpoint"/> class
    /// </summary>
    /// <param name="marketApi">The market API used to get current ticker information</param>
    /// <param name="symbolMapper">The symbol mapper</param>
    /// <param name="apiPrefix">The api prefix</param>
    /// <param name="securityProvider">The security provider</param>
    /// <param name="apiClient">The Bybit api client</param>
    public BybitTradeApiEndpoint(BybitMarketApiEndpoint marketApi, ISymbolMapper symbolMapper, string apiPrefix,
        ISecurityProvider securityProvider, BybitApiClient apiClient) : base(symbolMapper, apiPrefix, securityProvider,
        apiClient)
    {
        _marketApiClient = marketApi;
    }

    /// <summary>
    /// Cancels the order
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="order">The order to cancel</param>
    /// <returns>The order update response</returns>
    public BybitUpdateOrderResponse CancelOrder(BybitProductCategory category, Order order)
    {
        var cancelOrderRequest = new ByBitCancelOrderRequest
        {
            Category = category,
            Symbol = SymbolMapper.GetBrokerageSymbol(order.Symbol),
            OrderId = order.BrokerId.Single(),
            OrderFilter = GetOrderFilter(category, order)
        };

        return ExecutePostRequest<BybitUpdateOrderResponse>("/order/cancel", cancelOrderRequest);
    }

    /// <summary>
    /// Places a new order
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="order">The order to place</param>
    /// <returns>The order update response</returns>
    public BybitUpdateOrderResponse PlaceOrder(BybitProductCategory category, Order order)
    {
        var placeOrderRequest = CreateRequest(category, order);

        return ExecutePostRequest<BybitUpdateOrderResponse>("/order/create", placeOrderRequest);
    }

    /// <summary>
    /// Updates the order
    /// </summary>
    /// <param name="category">The product category</param>
    /// <param name="order">The order to update</param>
    /// <returns>The order update response</returns>
    public BybitUpdateOrderResponse UpdateOrder(BybitProductCategory category, Order order)
    {
        var updateOrderRequest = CreateRequest<ByBitUpdateOrderRequest>(category, order);
        updateOrderRequest.OrderId = order.BrokerId.FirstOrDefault();

        return ExecutePostRequest<BybitUpdateOrderResponse>("/order/amend", updateOrderRequest);
    }

    /// <summary>
    /// Query unfilled or partially filled orders in real-time. To query older order records, please use the order history interface.
    /// </summary>
    /// <param name="category">The product category</param>
    /// <returns>A enumerable of orders</returns>
    public IEnumerable<BybitOrder> GetOpenOrders(BybitProductCategory category)
    {
        var parameters = new Dictionary<string, string>();
        if (category == BybitProductCategory.Linear)
        {
            parameters["settleCoin"] = "USDT";
        }

        return FetchAll<BybitOrder>("/order/realtime", category, 50, parameters, true);
    }


    private ByBitPlaceOrderRequest CreateRequest(BybitProductCategory category, Order order)
    {
        return CreateRequest<ByBitPlaceOrderRequest>(category, order);
    }


    private T CreateRequest<T>(BybitProductCategory category, Order order) where T : ByBitPlaceOrderRequest, new()
    {
        if (order.Direction == OrderDirection.Hold) throw new NotSupportedException();
        var properties = order.Properties as BybitOrderProperties;
        var req = new T
        {
            Category = category,
            Side = order.Direction == OrderDirection.Buy ? OrderSide.Buy : OrderSide.Sell,
            Quantity = Math.Abs(order.Quantity),
            Symbol = SymbolMapper.GetBrokerageSymbol(order.Symbol),
            PositionIndex = 0,
            OrderFilter = GetOrderFilter(category, order),
            ReduceOnly = properties?.ReduceOnly,
        };

        //todo close on trigger?
        if (IsLimitType(order.Type))
        {
            req.TimeInForce = properties?.PostOnly == true ? TimeInForce.PostOnly : null;
        }

        switch (order)
        {
            case LimitOrder limitOrder:
                req.OrderType = OrderType.Limit;
                req.Price = limitOrder.LimitPrice;
                break;
            case MarketOrder:
                req.OrderType = OrderType.Market;
                if (category == BybitProductCategory.Spot && order.Direction == OrderDirection.Buy)
                {
                    var price = GetTickerPrice(category, order);
                    req.Quantity *=
                        price; //todo: spot market buys require price in quote currency is this a good place to do this?
                }

                break;
            case StopLimitOrder stopLimitOrder:
                req.OrderType = OrderType.Limit;
                req.TriggerPrice = stopLimitOrder.StopPrice;
                req.Price = stopLimitOrder.LimitPrice;
                var ticker = GetTickerPrice(category, order);
                req.TriggerDirection = req.TriggerPrice > ticker ? 1 : 2;

                break;
            case StopMarketOrder stopMarketOrder:
                req.OrderType = OrderType.Market;
                req.TriggerPrice = stopMarketOrder.StopPrice;
                ticker = GetTickerPrice(category, order);
                req.TriggerDirection = req.TriggerPrice > ticker ? 1 : 2;
                req.ReduceOnly = true;

                if (category == BybitProductCategory.Spot)
                {
                    if (order.Direction == OrderDirection.Buy)
                    {
                        req.Quantity *= stopMarketOrder.StopPrice;
                    }
                }

                break;
            case LimitIfTouchedOrder limitIfTouched:
                req.OrderType = OrderType.Limit;
                req.TriggerPrice = limitIfTouched.TriggerPrice;
                req.Price = limitIfTouched.LimitPrice;
                ticker = GetTickerPrice(category, order);
                req.TriggerDirection = req.TriggerPrice > ticker ? 1 : 2;

                break;
            default: throw new NotSupportedException($"Order type {order.Type.ToStringInvariant()} is not supported");
        }

        return req;
    }


    private decimal GetTickerPrice(BybitProductCategory category, Order order)
    {
        var security = SecurityProvider.GetSecurity(order.Symbol);
        var tickerPrice = order.Direction == OrderDirection.Buy ? security.AskPrice : security.BidPrice;
        if (tickerPrice == 0)
        {
            var brokerageSymbol = SymbolMapper.GetBrokerageSymbol(order.Symbol);
            var ticker = _marketApiClient.GetTicker(category, brokerageSymbol);
            if (ticker == null)
            {
                throw new KeyNotFoundException(
                    $"BinanceBrokerage: Unable to resolve currency conversion pair: {order.Symbol}");
            }

            tickerPrice = order.Direction == OrderDirection.Buy ? ticker.Ask1Price!.Value : ticker.Bid1Price!.Value;
        }

        return tickerPrice;
    }

    private bool IsLimitType(QuantConnect.Orders.OrderType orderType)
    {
        return orderType is (Orders.OrderType.Limit or Orders.OrderType.StopLimit or Orders.OrderType.LimitIfTouched);
    }

    private static OrderFilter? GetOrderFilter(BybitProductCategory category, Order order)
    {
        if (category != BybitProductCategory.Spot) return null;
        switch (order.Type)
        {
            case Orders.OrderType.StopLimit:
            case Orders.OrderType.StopMarket:
            case Orders.OrderType.LimitIfTouched:
                return OrderFilter.StopOrder;
            default:
                return default;
        }
    }
}