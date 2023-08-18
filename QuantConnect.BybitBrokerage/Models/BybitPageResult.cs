namespace QuantConnect.BybitBrokerage.Models;

public class BybitPageResult<T>
{
    public T[] List { get; set; }
    public string? NextPageCursor { get; set; }
}