using Newtonsoft.Json;

namespace QuantConnect.BybitBrokerage.Models.Messages;

public class BybitMessage
{
    public bool Success { get; set; }
    [JsonProperty("ret_msg")] public string ReturnMessage { get; set; }
    [JsonProperty("conn_id")] public string ConnectionId { get; set; }
    [JsonProperty("op")] public string Operation { get; set; }
}