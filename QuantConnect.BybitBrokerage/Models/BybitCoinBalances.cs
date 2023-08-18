using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Models;

public class BybitCoinBalances
{
    public Enums.AccountType AccountType { get; set; }
    public string MemberId { get; set; }
    [JsonProperty("balance")]
    public BybitCoinBalance[] Balances { get; set; }
}