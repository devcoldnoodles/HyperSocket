namespace HyperSocket.Http
{
    public delegate void HttpRouterEvent(HttpRequest request, HttpResponse response);
    public class HttpRouter
    {
        public string pattern;
        public string description;
        public HttpRouterEvent handle;
    }
}