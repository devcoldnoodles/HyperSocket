namespace HyperSocket.Http
{
    public delegate bool HttpRouterEvent(HttpRequest request, HttpResponse response);
    public delegate bool MultipartRequestEvent(HttpGeneralFormat format);
}