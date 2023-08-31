using System;
using Newtonsoft.Json;
using QuantConnect.BybitBrokerage.Converters;

namespace QuantConnect.BybitBrokerage.Models.Messages;

/// <summary>
/// Base websocket data message
/// </summary>
/// <typeparam name="T">The type of the business data</typeparam>
public class BybitDataMessage<T>
{
    /// <summary>
    /// The websocket topic this message belongs to
    /// </summary>
    public string Topic { get; set; }
    
    /// <summary>
    /// The message type 
    /// </summary>
    public BybitMessageType Type { get; set; }

    /// <summary>
    /// Message Time
    /// </summary>
    [JsonProperty("ts")]
    [JsonConverter(typeof(BybitTimeConverter))]
    public DateTime Time { get; set; }

    /// <summary>
    /// Business data
    /// </summary>
    public T Data { get; set; }
}