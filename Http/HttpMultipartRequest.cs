using System.Collections.Generic;

namespace HyperSocket.Http
{
    public class HttpMultipartRequest : HttpRequest
    {
        public List<HttpGeneralFormat> Forms;

        internal HttpMultipartRequest(HttpRequest context, List<HttpGeneralFormat> data) : base(context)
        {
            Forms = data;
        }
        public new void Dispose()
        {
            base.Dispose();
            foreach (var data in Forms)
                data.Dispose();
        }
    }
}