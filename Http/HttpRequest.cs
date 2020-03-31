using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HyperSocket.Http
{
    public class HttpRequest : HttpRequestHeader
    {
        public HttpMethod Method;
        public string URL;
        public Version Protocol;
        public Dictionary<string, object> Session { get; internal set; }
        public Match Match;
        internal object dataset;

        public string GetParameter(string param)
        {
            if (dataset != null && (Method == HttpMethod.Get || Method == HttpMethod.Post && ContentType.ToLower() == "application/x-www-form-urlencoded"))
                return ((Dictionary<string, string>)dataset)[param];
            return string.Empty;
        }
    }
}