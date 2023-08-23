using QuantConnect.BybitBrokerage.Models;
using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage;

public class BybitWSOrder : BybitOrder
{
    public BybitAccountCategory Category { get; set; }
}