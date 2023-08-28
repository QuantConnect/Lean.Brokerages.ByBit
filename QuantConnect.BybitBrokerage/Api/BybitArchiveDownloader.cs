using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;
using QuantConnect.Logging;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitArchiveDownloader
{
    private const string BaseAddress = "https://public.bybit.com";

    public IEnumerable<BybitHistTick> Download(BybitAccountCategory category, string symbol, DateTime from, DateTime to)
    {
        for (var i = from.Date; i <= to.Date; i = i.AddDays(1))
        {
            var res = Download(symbol, category, i);
            foreach (var tick in res)
            {
                if (tick.Time < from) continue;
                if (tick.Time > to) yield break;
                yield return tick;
            }
        }
    }

    public IEnumerable<BybitHistTick> Download(string symbol, BybitAccountCategory category, DateTime date)
    {
        if (category is not (BybitAccountCategory.Inverse or BybitAccountCategory.Linear or BybitAccountCategory.Spot))
        {
            throw new NotSupportedException("Only inverse, linear, and spot supported");
        }

        var categoryPath = category == BybitAccountCategory.Spot ? "spot" : "trading";
        var dateSeparator = category == BybitAccountCategory.Spot ? "_" : string.Empty;
        var endpoint = $"/{categoryPath}/{symbol}/{symbol}{dateSeparator}{date:yyyy-MM-dd}.csv.gz";

        var client = new RestClient(BaseAddress);
        var req = new RestRequest(endpoint);

        using var memoryStream = new MemoryStream();

        req.ResponseWriter = stream => stream.CopyTo(memoryStream);
        var resp = client.Execute(req);
        if (!resp.IsSuccessful) yield break; //todo error handling/logigng

        memoryStream.Position = 0;
        using (var gzip = new GZipStream(memoryStream, CompressionMode.Decompress))
        using (var streamReader = new StreamReader(gzip))
        {
            var line = streamReader.ReadLine(); //header
            line = streamReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                
                if (category == BybitAccountCategory.Spot)
                {
                    yield return ParseSpotTick(line, symbol);
                }
                else if(category is BybitAccountCategory.Inverse or BybitAccountCategory.Linear)
                {
                    var tick = ParseFuturesTick(line);
                    if (tick != null) yield return tick;
                }
                else
                {
                    throw new NotSupportedException();
                }
                line = streamReader.ReadLine();
            }
        }
    }


    private BybitHistTick ParseSpotTick(string line, string symbol)
    {
        var tick = new BybitHistTick();
        var split = line.Split(',');
        tick.Time = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(split[1])).UtcDateTime;
        tick.price = decimal.Parse(split[2], CultureInfo.InvariantCulture);
        tick.Size = decimal.Parse(split[3], CultureInfo.InvariantCulture);
        tick.Side = Enum.Parse<OrderSide>(split[4], true);
        tick.Symbol = symbol;
        return tick;
    }
    private BybitHistTick? ParseFuturesTick(string line)
    {
        var tick = new BybitHistTick();
        try
        {

            var split = line.Split(',');
            tick.Time = BybitCandleTimeConverter.Convert(decimal.Parse(split[0]));
            tick.Symbol = split[1];
            tick.Side = (OrderSide)Enum.Parse(typeof(OrderSide), split[2]);
            tick.Size = decimal.Parse(split[3], NumberStyles.Float, CultureInfo.InvariantCulture);
            tick.price = decimal.Parse(split[4], CultureInfo.InvariantCulture);
            tick.TickDirection = (TickDirection)Enum.Parse(typeof(TickDirection), split[5]);
            tick.TradeId = Guid.Parse(split[6]);
            //tick.GrossValue = decimal.Parse(split[7], CultureInfo.InvariantCulture);
            //tick.HomeNotional = decimal.Parse(split[8], CultureInfo.InvariantCulture);
            //tick.ForeignNotional = decimal.Parse(split[9], CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Log.Error(e,$"Error while parsing tick line: '{line}'");
            return null;
        }

        return tick;
    }
    public class BybitHistTick
    {
        public DateTime Time { get; set; }
        public string Symbol { get; set; }
        public OrderSide Side { get; set; }
        public decimal Size { get; set; }
        public decimal price { get; set; }
        public TickDirection TickDirection { get; set; }

        public Guid TradeId { get; set; }
        //public float GrossValue { get; set; }
        //public decimal HomeNotional { get; set; }
        //public decimal ForeignNotional { get; set; }
    }
}