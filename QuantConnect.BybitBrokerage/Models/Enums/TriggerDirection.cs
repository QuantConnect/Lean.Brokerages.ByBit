using System.Runtime.Serialization;

namespace QuantConnect.BybitBrokerage.Models.Enums;

/// <summary>
/// Conditional order trigger direction
/// </summary>
public enum TriggerDirection
{
    /// <summary>
    /// Rise triggers the order when price rises above the trigger value
    /// </summary>
    [EnumMember(Value = "1")] Rise = 1,
    
    /// <summary>
    /// Fall will trigger the order when price falls below the trigger value
    /// </summary>
    [EnumMember(Value = "2")] Fall = 2
}