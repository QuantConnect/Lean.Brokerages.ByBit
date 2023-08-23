using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models;

public class BybitTicker
{
    public string Symbol { get; set; }

    public TickDirection? TickDirection { get; set; }

    [JsonProperty("price24hPcnt")]
    public decimal? Price24HoursPercentage { get; set; }

    public decimal? LastPrice { get; set; }

    [JsonProperty("prevPrice24h")]
    public decimal? PreviousPrice24Hours { get; set; }

    [JsonProperty("highPrice24h")]
    public decimal? HighPrice24Hours { get; set; }

    [JsonProperty("lowPrice24h")]
    public decimal? LowPrice24Hours { get; set; }

    [JsonProperty("prevPrice1h")]
    public decimal? PreviousPrice1Hour { get; set; }

    public decimal? MarkPrice { get; set; }

    public decimal? IndexPrice { get; set; }

    public decimal? OpenInterest { get; set; }

    public decimal? OpenInterestValue { get; set; }

    [JsonProperty("turnover24h")]
    public decimal? Turnover24Hours { get; set; }

    [JsonProperty("volume24h")]
    public decimal? Volume24Hours { get; set; }

    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime? NextFundingTime { get; set; }

    public decimal? FundingRate { get; set; }

    public decimal? Bid1Price { get; set; }

    public decimal? Bid1Size { get; set; }

    public decimal? Ask1Price { get; set; }

    public decimal? Ask1Size { get; set; }
}