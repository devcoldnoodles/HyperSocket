namespace HyperSocket.Http
{
    public delegate void HttpRouterEvent(HttpRequest request, HttpResponse response);
    public delegate void MultipartRequestEvent(HttpGeneralFormat format);
}