namespace HyperSocket.Http
{
    public class HttpEventHandler : HttpRouter
    {
        private static HttpRouterEvent Defualt = (request, response) => false;
        public HttpRouterEvent Delete = Defualt;
        public HttpRouterEvent Get = Defualt;
        public HttpRouterEvent Head = Defualt;
        public HttpRouterEvent Options = Defualt;
        public HttpRouterEvent Post = Defualt;
        public HttpRouterEvent Put = Defualt;
        public HttpRouterEvent Trace = Defualt;

        public HttpEventHandler()
        {
            handle = (req, res) =>
            {
                switch (req.Method)
                {
                    case HttpMethod.Delete:
                        return Delete(req, res);
                    case HttpMethod.Get:
                        return Get(req, res);
                    case HttpMethod.Head:
                        return Head(req, res);
                    case HttpMethod.Options:
                        return Options(req, res);
                    case HttpMethod.Post:
                        return Post(req, res);
                    case HttpMethod.Put:
                        return Put(req, res);
                    case HttpMethod.Trace:
                        return Trace(req, res);
                    default:
                        return Defualt(req, res);
                }
            };
        }
    }
}