using System.IO;
using System.Text;
using System.Collections.Generic;

namespace HyperSocket.Http
{
    public static class Utility
    {
        internal static readonly byte[] CRLF = { 0x0D, 0x0A };
        internal static readonly byte[] DCRLF = { 0x0D, 0x0A, 0x0D, 0x0A };

        internal static int IndexOf(byte[] source, byte[] dest, int offset = 0)
        {
            int accum = 0;
            if (source.Length >= dest.Length && offset >= 0)
            {
                for (int index = offset; index < source.Length; ++index)
                {
                    if (accum == dest.Length)
                        return index;
                    else if (source[index] == dest[accum])
                        accum += 1;
                    else if (source[index] == dest[0])
                        accum = 1;
                    else
                        accum = 0;
                }
            }
            return -1;
        }
        internal static string UrlDecode(byte[] source, int offset, int size)
        {
            if (source == null || offset < 0 || size < 0 || source.Length - offset - size < 0)
                return null;
            byte[] copy = new byte[size - offset];
            int copy_index = 0;
            while (offset < size)
            {
                switch (source[offset])
                {
                    case (byte)'+':
                        copy[copy_index] = (byte)' ';
                        break;
                    case (byte)'%':
                        if (size - offset < 2)
                            return null;
                        copy[copy_index] = (byte)((source[offset + 1] - (source[offset + 1] >= 65 ? 55 : 48)) * 16);
                        copy[copy_index] += (byte)(source[offset + 2] - (source[offset + 2] >= 65 ? 55 : 48));
                        offset += 2;
                        break;
                    default:
                        copy[copy_index] = source[offset];
                        break;
                }
                ++offset;
                ++copy_index;
            }
            return Encoding.UTF8.GetString(copy, 0, copy_index);
        }
        internal static string UrlDecode(string source)
        {
            byte[] temp = Encoding.UTF8.GetBytes(source);
            return UrlDecode(temp, 0, temp.Length);
        }
        public static Dictionary<string, string> GetUrlEncoded(HttpRequest request)
        {
            var dataset = new Dictionary<string, string>();
            string content = null;
            switch (request.Method)
            {
                case HttpMethod.Get:
                    int questionIndex = request.URL.IndexOf('?');
                    if (questionIndex == -1)
                        return null;
                    content = request.URL.Substring(questionIndex + 1);
                    break;
                case HttpMethod.Post:
                    content = Encoding.ASCII.GetString((request.Content as MemoryStream)?.GetBuffer());
                    break;
            }
            foreach (string property in content.Split('&'))
            {
                int equalIndex = property.IndexOf("=");
                if (equalIndex == -1)
                    throw new InvalidDataException($"Invaild data '{property}'");
                dataset[UrlDecode(property.Substring(0, equalIndex).Trim())] = UrlDecode(property.Substring(equalIndex + 1).Trim());
            }
            return dataset;
        }
        public static List<HttpMultipartHeader> GetMultipartFormData(HttpRequest request)
        {
            ContentType contentType = new ContentType(request.ContentType);
            byte[] data = (request.Content as MemoryStream).GetBuffer();
            byte[] boundary = Encoding.ASCII.GetBytes("--" + contentType.Boundary);
            int headIndex = IndexOf(data, boundary, 0);
            int rearIndex = 0;
            List<HttpMultipartHeader> multipart = new List<HttpMultipartHeader>();
            while (true)
            {
                HttpMultipartHeader multipartdata = new HttpMultipartHeader();
                if (headIndex == -1 || (rearIndex = IndexOf(data, DCRLF, headIndex + CRLF.Length)) == -1)
                    break;
                using (StringReader reader = new StringReader(Encoding.ASCII.GetString(data, headIndex + CRLF.Length, rearIndex - headIndex - CRLF.Length)))
                {
                    string property;
                    while ((property = reader.ReadLine()) != null)
                    {
                        int colonIndex = property.IndexOf(":");
                        if (colonIndex < 0)
                            continue;
                        multipartdata.Header[property.Substring(0, colonIndex)] = property.Substring(colonIndex + 1);
                    }
                }
                if ((headIndex = IndexOf(data, boundary, rearIndex)) == -1)
                    throw new InvalidDataException();
                multipartdata.Content = new MemoryStream(headIndex - rearIndex - boundary.Length - CRLF.Length);
                multipartdata.Content.Write(data, rearIndex, headIndex - rearIndex - boundary.Length - CRLF.Length);
                multipart.Add(multipartdata);
                continue;
            }
            return multipart;
        }
    }
}