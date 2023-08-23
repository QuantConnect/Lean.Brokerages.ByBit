using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.BybitBrokerage.Models.Requests;
using QuantConnect.Orders;
using RestSharp;
using OrderType = QuantConnect.BybitBrokerage.Models.Enums.OrderType;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitTradeApiClient : BybitRestApiClient
{
    private readonly BybitMarketApiClient _marketApiClient;

    public BybitPlaceOrderResponse CancelOrder(BybitAccountCategory category, Order order)
    {
        var endpoint = $"{ApiPrefix}/order/cancel";
        var request = new RestRequest(endpoint, Method.POST);

        var req = new ByBitCancelOrderRequest
        {
            Category = category,
            Symbol = SymbolMapper.GetBrokerageSymbol(order.Symbol),
            OrderId = order.BrokerId.Single()
        };
            
        var body = JsonConvert.SerializeObject(req, SerializerSettings);
        request.AddParameter("", body, "application/json", ParameterType.RequestBody);
        
        AuthenticateRequest(request);
        
        var response  = ExecuteRequest(request);
        var result = EnsureSuccessAndParse<BybitPlaceOrderResponse>(response);
        return result;
    }

    public BybitPlaceOrderResponse PlaceOrder(BybitAccountCategory category, Order order)
    {
        var endpoint = $"{ApiPrefix}/order/create";
        var request = new RestRequest(endpoint, Method.POST);

        var placeOrderReq = CreateRequest(category, order);
        

        var body = JsonConvert.SerializeObject(placeOrderReq, SerializerSettings);
        request.AddParameter("", body, "application/json", ParameterType.RequestBody);
        
        AuthenticateRequest(request);

        var response = ExecuteRequest(request);
        var result = EnsureSuccessAndParse<BybitPlaceOrderResponse>(response);
        return result;
    }
    public IEnumerable<BybitOrder> GetOpenOrders(BybitAccountCategory category)
    {
        return FetchAll(category, FetchOpenOrders, x => x.List.Length < 50); //todo why is there a next page in the first place.... double check API
    }

    public BybitPlaceOrderResponse UpdateOrder(BybitAccountCategory category, Order order)
    {
        var endpoint = $"{ApiPrefix}/order/amend";

        var request = new RestRequest(endpoint, Method.POST);

        var placeOrderReq = CreateRequest<ByBitUpdateOrderRequest>(category, order);
        placeOrderReq.OrderId = order.BrokerId.FirstOrDefault();

        var body = JsonConvert.SerializeObject(placeOrderReq, SerializerSettings);
        request.AddParameter("", body, "application/json", ParameterType.RequestBody);
        
        AuthenticateRequest(request);

        var response = ExecuteRequest(request);
        var result = EnsureSuccessAndParse<BybitPlaceOrderResponse>(response);
        return result;
    }
    
    private ByBitPlaceOrderRequest CreateRequest(BybitAccountCategory category,Order order)
    {

        return CreateRequest<ByBitPlaceOrderRequest>(category, order);
    }
    private T CreateRequest<T>(BybitAccountCategory category, Order order) where T : ByBitPlaceOrderRequest, new()
    {
        if (order.Direction == OrderDirection.Hold) throw new NotSupportedException();
        var req = new T
        {
            Category = category,
            Side = order.Direction == OrderDirection.Buy ? OrderSide.Buy : OrderSide.Sell,
            Quantity = Math.Abs(order.Quantity),
            //OrderLinkId = order.Id.ToStringInvariant(), todo
            Symbol = SymbolMapper.GetBrokerageSymbol(order.Symbol),
            PositionIndex = 0
        };
        
        //todo reduce only
        switch (order)
        {
            case LimitOrder limitOrder:
                req.OrderType = OrderType.Limit;
                req.Price = limitOrder.LimitPrice;
                break;
            case MarketOrder mo:
                req.OrderType = OrderType.Market;
                break;
            case TrailingStopOrder trailingStopOrder:
                throw new NotImplementedException();

                break;
            case StopLimitOrder stopLimitOrder:
                req.OrderType = OrderType.Limit;
                req.TriggerPrice = stopLimitOrder.StopPrice;
                req.Price = stopLimitOrder.LimitPrice;
                //todo req.ReduceOnly = true;
                var ticker = _marketApiClient.GetTicker(category,order);
                req.TriggerDirection = req.TriggerPrice > ticker.LastPrice ? 1 : 2;


                break;
            case StopMarketOrder stopMarketOrder:
                req.OrderType = OrderType.Market;
                req.TriggerPrice = stopMarketOrder.StopPrice;
                //todo req.ReduceOnly = true;
                ticker = _marketApiClient.GetTicker(category,order);
                req.TriggerDirection = req.TriggerPrice > ticker.LastPrice ? 1 : 2;
                break;
            case LimitIfTouchedOrder limitIfTouched:
                req.OrderType = OrderType.Limit;
                req.TriggerPrice = limitIfTouched.TriggerPrice;
                req.Price = limitIfTouched.LimitPrice;
                ticker = _marketApiClient.GetTicker(category,order);
                req.TriggerDirection = req.TriggerPrice > ticker.LastPrice ? 1 : 2;

                break;
            default: throw new NotSupportedException($"Order type {order.Type.ToStringInvariant()} is not supported");
        }

        return req;
    }

    private BybitPageResult<BybitOrder> FetchOpenOrders(BybitAccountCategory category, string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/order/realtime";
        var request = new RestRequest(endpoint);

        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("limit", "50");
        if(category == BybitAccountCategory.Linear)
        {
            request.AddQueryParameter("settleCoin", "USDT"); //todo
        }else if(category == BybitAccountCategory.Spot)
        {
            //noop
        }
       
        if (cursor != null)
        {
            request.AddQueryParameter("cursor", "cursor", false);
        }

        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var orders = EnsureSuccessAndParse<BybitPageResult<BybitOrder>>(response);
        return orders;
    }

    public BybitTradeApiClient(BybitMarketApiClient marketApiClient, ISymbolMapper symbolMapper, string apiPrefix, IRestClient restClient, Action<IRestRequest> requestAuthenticator) : base(symbolMapper, apiPrefix, restClient, requestAuthenticator)
    {
        _marketApiClient = marketApiClient;
    }
}