using System;
using Newtonsoft.Json;

namespace RpcLibrary.Models
{
    public static class JsonRpcParser
    {
        public static bool ValidateJson(string json)
        {
            try
            {
                object request = JsonConvert.DeserializeObject<object>(json);
            }
            catch
            {
                return false;
            }

            return true;
        }

        internal static bool TryParseRequest<T>(string json, out Type type)
        {
            try
            {
                JsonRpcRequest<T> request = JsonConvert.DeserializeObject<JsonRpcRequest<T>>(json);
            }
            catch
            {
                type = null;
                return false;
            }

            type = typeof(T);
            return true;
        }

        internal static JsonRpcRequest<T> ParseRequest<T>(string json)
            => JsonConvert.DeserializeObject<JsonRpcRequest<T>>(json);

        public static JsonRpcResponce ParseResponce(string json)
            => JsonConvert.DeserializeObject<JsonRpcResponce>(json);
    }
}