using QuantConnect.BybitBrokerage.Models.Enums;

namespace QuantConnect.BybitBrokerage.Models.Messages;

/// <summary>
/// Order update
/// </summary>
public class BybitOrderUpdate : BybitOrder
{
    /// <summary>
    /// Product type
    /// </summary>
    public BybitProductCategory Category { get; set; }
}