using Newtonsoft.Json;

namespace RpcLibrary.Models
{
    public class JsonRpcResponce
    {
        [JsonProperty(Required = Required.Always)]
        public string jsonrpc { get; set; } = "2.0";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object result { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonRpcErrorObject error { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public int? id { get; set; }
    }
}