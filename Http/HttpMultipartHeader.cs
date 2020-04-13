namespace HyperSocket.Http
{
    public class HttpMultipartHeader : HttpGeneralFormat
    {
        public ContentDisposition ContentDisposition { get { return Header.TryGetValue("Content-Disposition", out object value) ? value as ContentDisposition : null; } internal set { Header["Content-Disposition"] = value; } }
    }
}