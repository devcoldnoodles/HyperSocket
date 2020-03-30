using System.Collections.Generic;

namespace HyperSocket.Http
{
    public class HttpMultipartRequest : HttpRequest
    {
        public List<HttpGeneralFormat> Forms { get { return Arguments as List<HttpGeneralFormat>;} }
    }
}