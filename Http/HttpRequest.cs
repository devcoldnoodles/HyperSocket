using System;
using System.Collections.Generic;

namespace HyperSocket.Http
{
    public class HttpRequest : HttpRequestHeader
    {
        public HttpMethod Method;
        public string URL;
        public Version Protocol;
        public Dictionary<string, HttpEventHandler> Session { get; internal set; }
        public Dictionary<string, string> Form { get; internal set; } = new Dictionary<string, string>();
        public MultipartRequestEvent OnReceiveMultipartFormData;
        public string Arguments;

        internal HttpRequest(HttpRequest context = null)
        {
            if (context != null)
            {
                Method = context.Method;
                URL = context.URL;
                Protocol = context.Protocol;
                Header = context.Header;
                Session = context.Session;
                Form = context.Form;
                Arguments = context.Arguments;
            }
        }

        public string GetParameter(string param)
        {
            return Form[param];
        }

        public T GetSession<T>(string path) where T : HttpEventHandler
        {
            if (!path.StartsWith("/"))
                throw new Exception("invalid path");
            return (T)(Session.TryGetValue("URLRoot" + path.TrimEnd('/'), out HttpEventHandler data) && data is T ? data : null);
        }
        public bool TryGetSession<T>(string path, out T value) where T : HttpEventHandler
        {
            if (!path.StartsWith("/"))
                throw new Exception("invalid path");
            if (Session.TryGetValue("URLRoot" + path.TrimEnd('/'), out HttpEventHandler data) && data is T)
            {
                value = (T)data;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}