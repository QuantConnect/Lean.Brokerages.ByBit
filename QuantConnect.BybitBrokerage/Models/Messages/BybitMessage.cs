using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Models.Messages;


/// <summary>
/// Websocket operation response message
/// </summary>
public class BybitOperationResponseMessage
{
    /// <summary>
    /// Whether the operation was successful 
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Return message
    /// </summary>
    [JsonProperty("ret_msg")] public string ReturnMessage { get; set; }
    
    /// <summary>
    /// Connection ID
    /// </summary>
    [JsonProperty("conn_id")] public string ConnectionId { get; set; }
    
    /// <summary>
    /// Executed opeartion
    /// </summary>
    [JsonProperty("op")] public string Operation { get; set; }
}