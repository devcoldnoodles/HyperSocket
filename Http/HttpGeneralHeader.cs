namespace HyperSocket.Http
{
    public class HttpGeneralHeader : HttpGeneralFormat
    {
        public string CacheControl
        {
            get => Header.TryGetValue("Cache-Control", out string value) ? value : string.Empty;
            set => Header["Cache-Control"] = value;
        }
        public string Connection
        {
            get => Header.TryGetValue("Connection", out string value) ? value : string.Empty;
            set => Header["Connection"] = value;
        }
        public string Date
        {
            get => Header.TryGetValue("Date", out string value) ? value : string.Empty;
            set => Header["Date"] = value;
        }
        public string Pragma
        {
            get => Header.TryGetValue("Pragma", out string value) ? value : string.Empty;
            set => Header["Pragma"] = value;
        }
        public string Via
        {
            get => Header.TryGetValue("Via", out string value) ? value : string.Empty;
            set => Header["Via"] = value;
        }
        public string Upgrade
        {
            get => Header.TryGetValue("Upgrade", out string value) ? value : string.Empty;
            set => Header["Upgrade"] = value;
        }
        public string TransferEncoding
        {
            get => Header.TryGetValue("Transfer-Encoding", out string value) ? value : string.Empty;
            set => Header["Transfer-Encoding"] = value;
        }
    }
}