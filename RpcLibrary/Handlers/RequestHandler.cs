using System;
using System.Reflection;
using System.Collections.Generic;
using RpcLibrary.Models;
using Newtonsoft.Json;

namespace RpcLibrary.Handlers
{
    internal static class RequestHandler
    {
        internal static string Handle(string json, out bool isNotification)
        {
            isNotification = false;
            string responce = JsonConvert.SerializeObject(GetResponce(json, out isNotification));
            return responce;
        }

        private static JsonRpcResponce GetResponce(string json, out bool isNotification)
        {
            isNotification = false;

            if (JsonRpcParser.ValidateJson(json) == false)
            {
                return SendError(-32700, "Parse error");
            }

            Type paramsType = GetRequestParamsType(json);
            if (paramsType == null)
            {
                return SendError(-32600, "Invalid Request");
            }

            if (paramsType == typeof(object[]))
            {
                return HandleRequest(json, out isNotification);
            }
            else
            {
                return HandleNamedRequest(json, out isNotification);
            }
        }
        private static Type GetRequestParamsType(string json)
        {
            Type type;
            if (JsonRpcParser.TryParseRequest<object[]>(json, out type) == true)
            {
                return type;
            }
            else if (JsonRpcParser.TryParseRequest<Dictionary<string, object>>(json, out type) == true)
            {
                return type;
            }
            else
            {
                return null;
            }
        }

        private static JsonRpcResponce HandleRequest(string json, out bool isNotification)
        {
            isNotification = false;

            JsonRpcRequest<object[]> request = JsonRpcParser.ParseRequest<object[]>(json);

            if (request.id == null)
            {
                isNotification = true;
            }

            MethodInfo method = ValidateMethod(request.method);

            if (method == null)
            {
                return SendError(-32601, "Method not found", request.id);
            }

            return ExecuteMethod(method, request.@params, request.id);
        }

        private static JsonRpcResponce HandleNamedRequest(string json, out bool isNotification)
        {
            isNotification = false;

            JsonRpcRequest<Dictionary<string, object>> request = JsonRpcParser.ParseRequest<Dictionary<string, object>>(json);

            if (request.id == null)
            {
                isNotification = true;
            }

            MethodInfo method = ValidateMethod(request.method);

            if (method == null)
            {
                return SendError(-32601, "Method not found", request.id);
            }

            return ExecuteNamedMethod(method, request.@params, request.id);
        }

        private static MethodInfo ValidateMethod(string methodName) 
            => RpcServer.Procedures.GetType().GetMethod(methodName);

        private static JsonRpcResponce ExecuteNamedMethod(MethodInfo method, Dictionary<string, object> @params, int? id)
        {
            if (method.GetParameters().Length != @params.Count)
            {
                return SendError(-32602, "Invalid params", id);
            }

            object[] objParams = new object[method.GetParameters().Length];

            int count = 0;
            foreach (ParameterInfo param in method.GetParameters())
            {
                int i = count;
                foreach (KeyValuePair<string, object> keyValue in @params)
                {
                    if (keyValue.Key == param.Name)
                    {
                        objParams[count] = keyValue.Value;
                        count++;
                        break;
                    }
                }
                if (i == count)
                {
                    return SendError(-32602, "Invalid params", id);
                }
            }

            return ExecuteMethod(method, objParams, id);
        }

        private static JsonRpcResponce ExecuteMethod(MethodInfo method, object[] @params, int? id)
        {
            object result = null;

            try
            {
                result = method.Invoke(RpcServer.Procedures, @params);
            }
            catch (ArgumentException)
            {
                return SendError(-32602, "Invalid params", id);
            }
            catch (TargetParameterCountException)
            {
                return SendError(-32602, "Invalid params", id);
            }
            catch
            {
                return SendError(-32603, "Internal error", id);
            }

            return new JsonRpcResponce()
            {
                result = result,
                id = id
            };
        }

        private static JsonRpcResponce SendError(int errorCode, string errorMessage, int? id = null) => new JsonRpcResponce()
        {
            error = new JsonRpcErrorObject()
            {
                code = errorCode,
                message = errorMessage,
                date = DateTime.Now
            },
            id = id
        };
    }
}