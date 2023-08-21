using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using QuantConnect.BybitBrokerage.Converters;
using QuantConnect.BybitBrokerage.Models.Enums;
using RestSharp;

namespace QuantConnect.BybitBrokerage.Api;

public class BybitArchiveDownloader
{
    private const string _baseAddress = "https://public.bybit.com";

    public IEnumerable<BybitHistTick> Download(BybitAccountCategory category, string symbol, DateTime from, DateTime to)
    {
        for (var i = from.Date; i <= to.Date; i = i.AddDays(1))
        {
            var res = Download(symbol, category, i);
            foreach (var tick in res)
            {
                if(tick.Time < from) continue;
                if(tick.Time > to) yield break;
                yield return tick;
            }
        }
    }
    public IEnumerable<BybitHistTick> Download(string symbol, BybitAccountCategory category, DateTime date)
    {

        category = BybitAccountCategory.Linear;
        if (category is not (BybitAccountCategory.Inverse or BybitAccountCategory.Linear))
        {
            throw new NotSupportedException("Only inverse and linear supported");
        }

        var endpoint = $"/trading/{symbol}/{symbol}{date:yyyy-MM-dd}.csv.gz";

        var client = new RestClient(_baseAddress);
        var req = new RestRequest(endpoint);
        //req.AddDecompressionMethod(DecompressionMethods.Deflate);
        //req.AddDecompressionMethod(DecompressionMethods.GZip);
        //req.AddDecompressionMethod(DecompressionMethods.Brotli);

        using (var memoryStream = new MemoryStream())
        {
            req.ResponseWriter = stream => stream.CopyTo(memoryStream);
            var resp =client.Execute(req);
            if(!resp.IsSuccessful) yield break; //todo error handling/logigng
            
            memoryStream.Position = 0;
            using(var gzip = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gzip))
            {
                var line = streamReader.ReadLine(); //header
                line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var tick = new BybitHistTick();
                    var split = line.Split(',');
                    tick.Time = BybitCandleTimeConverter.Convert(decimal.Parse(split[0], CultureInfo.InvariantCulture));
                    tick.Symbol = split[1];
                    tick.Side = (OrderSide) Enum.Parse(typeof(OrderSide), split[2]);
                    tick.Size = decimal.Parse(split[3], CultureInfo.InvariantCulture);
                    tick.price = decimal.Parse(split[4], CultureInfo.InvariantCulture);
                    tick.TickDirection = (TickDirection)Enum.Parse(typeof(TickDirection), split[5]);
                    tick.TradeId = Guid.Parse(split[6]);
                    //tick.GrossValue = decimal.Parse(split[7], CultureInfo.InvariantCulture);
                    //tick.HomeNotional = decimal.Parse(split[8], CultureInfo.InvariantCulture);
                    //tick.ForeignNotional = decimal.Parse(split[9], CultureInfo.InvariantCulture);

                    yield return tick;
                    line = streamReader.ReadLine();
                }
            }
            

        }
         








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