using System;
using System.Collections.Generic;

namespace HyperSocket.Http
{
    public class HttpRequest : HttpRequestHeader
    {
        public HttpMethod Method;
        public string URL;
        public Version Protocol;
        public Dictionary<string, object> Session { get; internal set; }
        internal object Arguments;

        public object GetParameter(string param)
        {
            switch (ContentType.ToLower())
            {
                case "application/x-www-form-urlencoded":
                    return ((Dictionary<string, string>)Arguments)[param];
                case "multipart/form-data":
                    return Arguments;
                default:
                    return null;
            }
        }
    }
}