namespace HyperSocket.Http
{
    public delegate void HttpRouterEvent(HttpRequest request, HttpResponse response);
    public class HttpRouter
    {
        public string Pattern { get; set; }
        public HttpRouterEvent Handle { get; set; }
    }
}