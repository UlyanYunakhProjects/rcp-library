using System;
using Newtonsoft.Json;

namespace RpcLibrary.Models
{
    public class JsonRpcErrorObject
    {
        [JsonProperty(Required = Required.Always)]
        public int code { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? date {get; set;} = null;
    }
}