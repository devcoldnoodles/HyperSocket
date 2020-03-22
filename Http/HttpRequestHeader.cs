namespace HyperSocket.Http
{
    public class HttpRequestHeader : HttpEntityHeader
    {
        public string Accept
        {
            get => Header.TryGetValue("Accept", out string value) ? value : string.Empty;
            set => Header["Accept"] = value;
        }
        public string AcceptCharset
        {
            get => Header.TryGetValue("Accept-Charset", out string value) ? value : string.Empty;
            set => Header["Accept-Charset"] = value;
        }
        public string AcceptEncoding
        {
            get => Header.TryGetValue("Accept-Encoding", out string value) ? value : string.Empty;
            set => Header["Accept-Encoding"] = value;
        }
        public string AcceptLanguage
        {
            get => Header.TryGetValue("Accept-Language", out string value) ? value : string.Empty;
            set => Header["Accept-Language"] = value;
        }
        public string Authorization
        {
            get => Header.TryGetValue("Authorization", out string value) ? value : string.Empty;
            set => Header["Authorization"] = value;
        }
        /* Unofficial Heder */
        public string Cookie
        {
            get => Header.TryGetValue("Cookie", out string value) ? value : string.Empty;
            set => Header["Cookie"] = value;
        }
        public string From
        {
            get => Header.TryGetValue("From", out string value) ? value : string.Empty;
            set => Header["From"] = value;
        }
        public string Host
        {
            get => Header.TryGetValue("Host", out string value) ? value : string.Empty;
            set => Header["Host"] = value;
        }
        public string IfModifiedSince
        {
            get => Header.TryGetValue("If-Modified-Since", out string value) ? value : string.Empty;
            set => Header["If-Modified-Since"] = value;
        }
        public string IfMatch
        {
            get => Header.TryGetValue("If-Match", out string value) ? value : string.Empty;
            set => Header["If-Match"] = value;
        }
        public string IfNoneMatch
        {
            get => Header.TryGetValue("If-None-Match", out string value) ? value : string.Empty;
            set => Header["If-None-Match"] = value;
        }
        public string IfRange
        {
            get => Header.TryGetValue("If-Range", out string value) ? value : string.Empty;
            set => Header["From"] = value;
        }
        public string IfUnmodifiedSince
        {
            get => Header.TryGetValue("If-Unmodified-Since", out string value) ? value : string.Empty;
            set => Header["If-Unmodified-Since"] = value;
        }
        public string MaxForwards
        {
            get => Header.TryGetValue("Max-Forwards", out string value) ? value : string.Empty;
            set => Header["Max-Forwards"] = value;
        }
        public string ProxyAuthorization
        {
            get => Header.TryGetValue("Proxy-Authorization", out string value) ? value : string.Empty;
            set => Header["Proxy-Authorization "] = value;
        }
        public string Range
        {
            get => Header.TryGetValue("Range", out string value) ? value : string.Empty;
            set => Header["Range"] = value;
        }
        public string Referer
        {
            get => Header.TryGetValue("Referer", out string value) ? value : string.Empty;
            set => Header["Referer"] = value;
        }
        /* WebSocket Header */
        public string SecWebSocketKey
        {
            get => Header.TryGetValue("Sec-WebSocket-Key", out string value) ? value : string.Empty;
            set => Header["Sec-WebSocket-Key"] = value;
        }
        public string UserAgent
        {
            get => Header.TryGetValue("User-Agent", out string value) ? value : string.Empty;
            set => Header["User-Agent"] = value;
        }
    }
}