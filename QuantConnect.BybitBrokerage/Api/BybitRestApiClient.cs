using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.BybitBrokerage.Models.Requests;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Util;
using RestSharp;
using OrderType = QuantConnect.BybitBrokerage.Models.Enums.OrderType;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitRestApiClient : IDisposable
{
    protected static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Converters = new List<JsonConverter>(){new ByBitKlineJsonConverter(), new StringEnumConverter(), new BybitDecimalStringConverter()},
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly ISymbolMapper _symbolMapper;
    private readonly ISecurityProvider _securityProvider;
    private readonly RestClient _restClient;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly HMACSHA256 _hmacsha256;

    protected string ApiPrefix { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="BybitRestApiClient"/> class.
    /// </summary>
    /// <param name="symbolMapper">The symbol mapper.</param>
    /// <param name="securityProvider">The holdings provider.</param>
    /// <param name="apiKey">The Binance API key</param>
    /// <param name="apiSecret">The The Binance API secret</param>
    /// <param name="restApiUrl">The Bina
    /// nce API rest url</param>
    public BybitRestApiClient(
        ISymbolMapper symbolMapper,
        ISecurityProvider securityProvider,
        string apiKey,
        string apiSecret,
        string restApiUrl)
    {
        _symbolMapper = symbolMapper;
        _securityProvider = securityProvider;
        _restClient = new RestClient(restApiUrl);
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret ?? string.Empty));

        ApiPrefix = "/v5";
    }


    public BybitTicker GetTicker(BybitAccountCategory category, string symbol)
    {
        var endpoint = $"{ApiPrefix}/market/tickers";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("symbol", symbol);

        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitPageResult<BybitTicker>>(response).List.Single();
    }

    public BybitTicker GetTicker(BybitAccountCategory category, Order order)
    {
        var symbol = _symbolMapper.GetBrokerageSymbol(order.Symbol);
        return GetTicker(category, symbol);
    }
    
    public BybitPlaceOrderResponse CancelOrder(BybitAccountCategory category, Order order)
    {
        var endpoint = $"{ApiPrefix}/order/cancel";
        var request = new RestRequest(endpoint, Method.POST);

        var req = new ByBitCancelOrderRequest
        {
            Category = category,
            Symbol = _symbolMapper.GetBrokerageSymbol(order.Symbol),
            OrderId = order.BrokerId.Single()
        };
            
        var body = JsonConvert.SerializeObject(req, SerializerSettings);
        request.AddParameter("", body, "application/json", ParameterType.RequestBody);
        
        AuthenticateRequest(request);
        
        var response  = _restClient.Execute(request);
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

        var response  = _restClient.Execute(request);
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
            Symbol = _symbolMapper.GetBrokerageSymbol(order.Symbol),
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
                var ticker = GetTicker(category,order);
                req.TriggerDirection = req.TriggerPrice > ticker.LastPrice ? 1 : 2;


                break;
            case StopMarketOrder stopMarketOrder:
                req.OrderType = OrderType.Market;
                req.TriggerPrice = stopMarketOrder.StopPrice;
               //todo req.ReduceOnly = true;
                 ticker = GetTicker(category,order);
                req.TriggerDirection = req.TriggerPrice > ticker.LastPrice ? 1 : 2;
                break;
            case LimitIfTouchedOrder limitIfTouched:
                req.OrderType = OrderType.Limit;
                req.TriggerPrice = limitIfTouched.TriggerPrice;
                req.Price = limitIfTouched.LimitPrice;
                ticker = GetTicker(category,order);
                req.TriggerDirection = req.TriggerPrice > ticker.LastPrice ? 1 : 2;

                break;
            default: throw new NotSupportedException($"Order type {order.Type.ToStringInvariant()} is not supported");
        }

        return req;
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

        var response  = _restClient.Execute(request);
        var result = EnsureSuccessAndParse<BybitPlaceOrderResponse>(response);
        return result;
    }
    
    public IEnumerable<BybitInstrumentInfo> GetInstrumentInfo(BybitAccountCategory category)
    {
        return FetchAll(category, FetchInstruementInfo);
    }

    public IEnumerable<BybitPositionInfo> GetPositions(BybitAccountCategory category)
    {
        return FetchAll(category, FetchPositionInfo, result => result.List.Length < 200);
    }
    private BybitPageResult<BybitPositionInfo> FetchPositionInfo(BybitAccountCategory category,string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/position/list";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("settleCoin", "USDT"); //todo
        request.AddQueryParameter("limit", "200");
        if (cursor != null)
        {
            request.AddQueryParameter("cursor", cursor, false);
        }
        
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);
        return EnsureSuccessAndParse<BybitPageResult<BybitPositionInfo>>(response);
    }

    public BybitBalance GetWalletBalances(BybitAccountCategory category)
    {
        var endpoint = $"{ApiPrefix}/account/wallet-balance";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("accountType", category == BybitAccountCategory.Inverse ? "CONTRACT":"UNIFIED");

      
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var balance =  EnsureSuccessAndParse<BybitPageResult<BybitBalance>>(response).List.Single();
        return balance;
    }

    public BybitCoinBalances GetCoinBalances()
    {
        
        var endpoint = $"{ApiPrefix}/asset/transfer/query-account-coins-balance";

        var request = new RestRequest(endpoint);
        request.AddQueryParameter("accountType", "UNIFIED");

      
        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var balances =  EnsureSuccessAndParse<BybitCoinBalances>(response);
        return balances;
    }

    private T[] FetchAll<T>(BybitAccountCategory category, Func<BybitAccountCategory, string, BybitPageResult<T>> fetch, Predicate<BybitPageResult<T>> @break  = null)
    {
        var results = new List<T>();
        string cursor = null;
        do
        {
            var result = fetch(category, cursor);
            results.AddRange(result.List);
            cursor = result.NextPageCursor;
            if(@break?.Invoke(result) ?? false) break;
        } while (!string.IsNullOrEmpty(cursor));

        return results.ToArray();
    }

    private BybitPageResult<BybitInstrumentInfo> FetchInstruementInfo(BybitAccountCategory category,
        string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/market/instruments-info";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("limit", "1000");

        if (cursor != null)
        {
            request.AddQueryParameter("cursor", cursor);
        }

        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitPageResult<BybitInstrumentInfo>>(response);
    }

    private ByBitKLine[] FetchKLines(BybitAccountCategory category, string symbol,Resolution resolution,  long? start = null, long? end = null)
    {
        var endpoint = $"{ApiPrefix}/market/kline";
        var request = new RestRequest(endpoint);
        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("symbol", symbol);
        request.AddQueryParameter("interval", GetIntervalString(resolution));
        request.AddQueryParameter("limit", "200");
        if (start.HasValue)
        {
            request.AddQueryParameter("start", start.ToStringInvariant());
        }

        if (end.HasValue)
        {
            request.AddQueryParameter("end", end.ToStringInvariant());
        }

        var response = ExecuteRequest(request);

        return EnsureSuccessAndParse<BybitPageResult<ByBitKLine>>(response).List;
    }


    public IEnumerable<ByBitKLine> GetKLines(BybitAccountCategory category,string symbol, Resolution resolution, long from, long to)
    { 
        var msToNextBar = (long) resolution.ToTimeSpan().TotalMilliseconds;
        var maxTimeSpan = 199 * (long)resolution.ToTimeSpan().TotalMilliseconds;
        while (from < to)
        {
            var curTo = from + maxTimeSpan;
            var response = FetchKLines(category, symbol, resolution, from, curTo).Reverse().ToArray();
            if(response.Length == 0) yield break;
            foreach (var kLine in response)
            {
                if (kLine.OpenTime < to)
                {
                    yield return kLine;
                }
                else
                {
                    yield break;
                }
            }
            from = response.Last().OpenTime + msToNextBar;
        }
    }
    public IEnumerable<ByBitKLine> GetKLines(BybitAccountCategory category,string symbol, Resolution resolution, DateTime from, DateTime to)
    {
        return GetKLines(category,symbol, resolution, (long) Time.DateTimeToUnixTimeStampMilliseconds(from),(long) Time.DateTimeToUnixTimeStampMilliseconds(to));


    }
    private static string GetIntervalString(Resolution resolution)
    {
        return resolution switch
        {
            Resolution.Daily => "D",
            Resolution.Hour => "60",
            Resolution.Minute => "1",
            Resolution.Second => throw new NotSupportedException("Smallest supported timeframe is 1 minute"),
            Resolution.Tick => throw new NotSupportedException("Smallest supported timeframe is 1 minute"),
            _ => throw new ArgumentOutOfRangeException(nameof(resolution))
        };
    }
    private BybitPageResult<BybitOrder> FetchOpenOrders(BybitAccountCategory category, string cursor = null)
    {
        var endpoint = $"{ApiPrefix}/order/realtime";
        var request = new RestRequest(endpoint);

        request.AddQueryParameter("category", category.ToStringInvariant().ToLowerInvariant());
        request.AddQueryParameter("limit", "50");
        request.AddQueryParameter("settleCoin", "USDT");//todo 
       
        if (cursor != null)
        {
            request.AddQueryParameter("cursor", "cursor", false);
        }

        AuthenticateRequest(request);
        var response = ExecuteRequest(request);

        var orders = EnsureSuccessAndParse<BybitPageResult<BybitOrder>>(response);
        return orders;
    }

    private T EnsureSuccessAndParse<T>(IRestResponse response)
    {
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new Exception("ByBitApiClient request failed: " +
                                $"[{(int)response.StatusCode}] {response.StatusDescription}, " +
                                $"Content: {response.Content}, ErrorMessage: {response.ErrorMessage}");
        }

        ByBitResponse<T> byBitResponse;
        try
        {
            byBitResponse = JsonConvert.DeserializeObject<ByBitResponse<T>>(response.Content, SerializerSettings);
        }
        catch (Exception e)
        {
            throw new Exception("ByBitApiClient failed deserializing response: " +
                                $"[{(int)response.StatusCode}] {response.StatusDescription}, " +
                                $"Content: {response.Content}, ErrorMessage: {response.ErrorMessage}", e);
        }

        if (byBitResponse?.ReturnCode != 0)
        {
            throw new Exception("ByBitApiClient request failed: " +
                                $"[{(int)response.StatusCode}] {response.StatusDescription}, " +
                                $"Content: {response.Content}, ErrorCode: {byBitResponse.ReturnCode} ErrorMessage: {byBitResponse.ReturnMessage}");
        }

        if (Log.DebuggingEnabled)
        {
            Log.Debug(
                $"Bybit request for {response.Request.Resource} executed successfully. Response: {response.Content}");
        }

        return byBitResponse.Result;
    }

    private IRestResponse ExecuteRequest(IRestRequest request)
    {
        //todo rate limit
        return _restClient.Execute(request);
    }

    private void AuthenticateRequest(IRestRequest request)
    {
        string sign;
        if (request.Method == Method.GET)
        {
            var queryParams = request.Parameters
                .Where(x => x.Type is ParameterType.QueryString or ParameterType.QueryStringWithoutEncode)
                //.OrderBy(x => x.Name)
                .Select(x => $"{x.Name}={x.Value}")
                .ToArray(); //todo do not decode everything

            sign = string.Join("&", queryParams);
            

        }else if (request.Method == Method.POST)
        {
            var body = request.Parameters.Single(x => x.Type == ParameterType.RequestBody).Value;
            sign = (string)body;
        }
        else
        {
            throw new NotSupportedException();
        }
        
        var nonce = GetNonce();
        var sToSign = $"{nonce}{_apiKey}{sign}";
        var signed = Sign(sToSign, _hmacsha256);
        request.AddHeader("X-BAPI-SIGN", signed);
        request.AddHeader("X-BAPI-API-KEY", _apiKey);
        request.AddHeader("X-BAPI-TIMESTAMP", nonce);
        request.AddHeader("X-BAPI-SIGN-TYPE", "2");

    }

    private static string Sign(string queryString, HMACSHA256 hmacsha256)
    {
        var messageBytes = Encoding.UTF8.GetBytes(queryString);

        var computedHash = hmacsha256.ComputeHash(messageBytes);
        return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        var hex = new StringBuilder(computedHash.Length * 2);
        foreach (var b in computedHash)
        {
            hex.Append($"{b:x2}");
        }

        return hex.ToString();
    }

    private static string GetNonce()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
    }
    
    public object AuthenticateWS()
    {
        var expires = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeMilliseconds(); //todo amount of seconds
        var signString = $"GET/realtime{expires}";
        var signed = Sign(signString, _hmacsha256);
        return new
        {
            op = "auth",
            args = new object[]
            {
                _apiKey,
                expires,
                signed
            }
        };
    }
    public void Dispose()
    {
        _hmacsha256.Dispose();
    }
}