using Newtonsoft.Json;
using System.Collections.Generic;

namespace RpcLibrary.Models
{
    public class JsonRpcRequest<T>
    {
        [JsonProperty(Required = Required.Always)]
        public string jsonrpc { get; set; } = "2.0";

        [JsonProperty(Required = Required.Always)]
        public string method { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T @params { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? id { get; set; } = null;

        private JsonRpcRequest()
        {}

        public static JsonRpcRequest<T> CreateRequest()
        {
            if (typeof(T) == typeof(object[]) || typeof(T) == typeof(Dictionary<string, object>))
                return new JsonRpcRequest<T>();
            else return null;
        }
    }
}
