namespace QuantConnect.BybitBrokerage.Models;

public class BybitCoinBalance
{
    public string Coin { get; set; }
    public decimal TransferBalance { get; set; }
    public decimal WalletBalance { get; set; }
    public decimal Bonus { get; set; }
}