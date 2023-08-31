namespace QuantConnect.BybitBrokerage.Models;

/// <summary>
/// Bybit business data wrapper for results with pagination support
/// </summary>
/// <typeparam name="T">The business object type</typeparam>
public class BybitPageResult<T>
{
    /// <summary>
    /// The result items
    /// </summary>
    public T[] List { get; set; }
    
    /// <summary>
    /// Cursor for the next page, if empty or null it's the last page
    /// </summary>
    public string NextPageCursor { get; set; }
}