namespace HyperSocket.Http
{
    public class HttpEventHandler : HttpRouter
    {
        private static HttpRouterEvent Defualt = (request, response) => {};
        public HttpRouterEvent Delete = Defualt;
        public HttpRouterEvent Get = Defualt;
        public HttpRouterEvent Head = Defualt;
        public HttpRouterEvent Options = Defualt;
        public HttpRouterEvent Post = Defualt;
        public HttpRouterEvent Put = Defualt;
        public HttpRouterEvent Trace = Defualt;

        public HttpEventHandler()
        {
            Handle = (req, res) =>
            {
                switch (req.Method)
                {
                    case HttpMethod.Delete:
                        Delete(req, res);
                        break;
                    case HttpMethod.Get:
                        Get(req, res);
                        break;
                    case HttpMethod.Head:
                        Head(req, res);
                        break;
                    case HttpMethod.Options:
                        Options(req, res);
                        break;
                    case HttpMethod.Post:
                        Post(req, res);
                        break;
                    case HttpMethod.Put:
                        Put(req, res);
                        break;
                    case HttpMethod.Trace:
                        Trace(req, res);
                        break;
                    default:
                        Defualt(req, res);
                        break;
                }
            };
        }
    }
}