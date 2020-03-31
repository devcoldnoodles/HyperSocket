using System.Collections.Generic;

namespace HyperSocket.Http
{
    public class HttpMultipartRequest : HttpRequest
    {
        public new List<HttpGeneralFormat> GetParameter(string param = null)
        {
            if (ContentType.ToLower() != "multipart/form-data")
                return null;
            if (param == null)
                return dataset as List<HttpGeneralFormat>;
            List<HttpGeneralFormat> temp = new List<HttpGeneralFormat>();
            foreach (var data in (List<HttpGeneralFormat>)dataset)
                if (data.Header.ContainsKey(param))
                    temp.Add(data);
            return temp;
        }
    }
}