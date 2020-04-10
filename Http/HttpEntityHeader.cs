namespace HyperSocket.Http
{
    public class HttpEntityHeader : HttpGeneralHeader
    {
        public string Allow
        {
            get => Header.TryGetValue("Allow", out object value) ? value.ToString() : string.Empty;
            set => Header["Allow"] = value;
        }
        public string ContentBase
        {
            get => Header.TryGetValue("Content-Base", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-Base"] = value;
        }
        public string ContentEncoding
        {
            get => Header.TryGetValue("Content-Encoding", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-Encoding"] = value;
        }
        public string ContentLanguage
        {
            get => Header.TryGetValue("Content-Language", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-Language"] = value;
        }
        public string ContentLength
        {
            get => Header.TryGetValue("Content-Length", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-Length"] = value;
        }
        public string ContentLocation
        {
            get => Header.TryGetValue("Content-Location", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-Location"] = value;
        }
        public string ContentMD5
        {
            get => Header.TryGetValue("Content-MD5", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-MD5"] = value;
        }
        public string ContentRange
        {
            get => Header.TryGetValue("Content-Range", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-Range"] = value;
        }
        public string ContentType
        {
            get => Header.TryGetValue("Content-Type", out object value) ? value.ToString() : string.Empty;
            set => Header["Content-Type"] = value;
        }
        public string ETag
        {
            get => Header.TryGetValue("ETag", out object value) ? value.ToString() : string.Empty;
            set => Header["ETag"] = value;
        }
        public string Expires
        {
            get => Header.TryGetValue("Expires", out object value) ? value.ToString() : string.Empty;
            set => Header["Expires"] = value;
        }
        public string LastModified
        {
            get => Header.TryGetValue("Last-Modified", out object value) ? value.ToString() : string.Empty;
            set => Header["Last-Modified"] = value;
        }
    }
}