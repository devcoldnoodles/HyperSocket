namespace HyperSocket.Http
{
    public class HttpResponseHeader : HttpEntityHeader
    {
        public string Age
        {
            get => Header.TryGetValue("Age", out string value) ? value : string.Empty;
            set => Header["Age"] = value;
        }
        public string KeepAlive
        {
            get => Header.TryGetValue("Keep-Alive", out string value) ? value : string.Empty;
            set => Header["Keep-Alive"] = value;
        }
        public string Location
        {
            get => Header.TryGetValue("Location", out string value) ? value : string.Empty;
            set => Header["Location"] = value;
        }
        public string ProxyAuthenticate
        {
            get => Header.TryGetValue("Proxy-Authenticate", out string value) ? value : string.Empty;
            set => Header["Proxy-Authenticate"] = value;
        }
        public string Public
        {
            get => Header.TryGetValue("Public", out string value) ? value : string.Empty;
            set => Header["Public"] = value;
        }
        public string RetryAfter
        {
            get => Header.TryGetValue("Retry-After", out string value) ? value : string.Empty;
            set => Header["Retry-After"] = value;
        }
        
        /* Unofficial Heder */
        public string SecWebSocketAccept
        {
            get => Header.TryGetValue("Sec-WebSocket-Accept", out string value) ? value : string.Empty;
            set => Header["Sec-WebSocket-Accept"] = value;
        }
        /* Unofficial Heder */
        public string SetCookie
        {
            get => Header.TryGetValue("Set-Cookie", out string value) ? value : string.Empty;
            set => Header["Set-Cookie"] = value;
        }
        public string Server
        {
            get => Header.TryGetValue("Server", out string value) ? value : string.Empty;
            set => Header["Server"] = value;
        }
        public string Vary
        {
            get => Header.TryGetValue("Vary", out string value) ? value : string.Empty;
            set => Header["Vary"] = value;
        }
        public string Warning
        {
            get => Header.TryGetValue("Warning", out string value) ? value : string.Empty;
            set => Header["Warning"] = value;
        }
        public string WWWAuthenticate
        {
            get => Header.TryGetValue("WWW-Authenticate", out string value) ? value : string.Empty;
            set => Header["WWW-Authenticate"] = value;
        }
    }
}