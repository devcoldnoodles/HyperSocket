using System;
using System.Text;

namespace HyperSocket.Http
{
    public class HttpParser
    {
        static readonly byte[] SPACE = { 0x20 };
        static readonly byte[] CR = { 0x0D };
        static readonly byte[] LF = { 0x0A };
        static readonly byte[] CRLF = { 0x0D, 0x0A };
        static readonly byte[] DCRLF = { 0x0D, 0x0A, 0x0D, 0x0A };
        static readonly byte[] BINARY = Encoding.ASCII.GetBytes("\r\n\r\n :HTTP/");
        //const byte COLON = 0x32;
        
        internal static int IndexOf(byte[] source, int source_offset, int source_size, byte dest)
        {
            if (source == null ||
            source_offset < 0 ||
            source_size <= 0 ||
            source.Length - source_size < 0)
                return -1;
            while(source[source_offset] != dest && ++source_offset < source_size);
            return source_offset == source_size ? - 1 : source_offset;
        }
        internal static int IndexOf(byte[] source, int source_offset, int source_size, byte[] dest, int dest_offset, int dest_size)
        {
            if (source == null ||
            dest == null ||
            source_offset < 0 ||
            dest_offset < 0 ||
            source_size <= 0 ||
            dest_size <= 0 ||
            source.Length - source_size < 0 ||
            dest.Length - dest_size < 0 ||
            (source.Length - source_offset - source_size) - (dest.Length - dest_offset - dest_size) < 0)
                return -1;
            for (int index = source_offset, accum = 0; index < source_size; ++index)
                if (accum == dest_size - dest_offset)
                    return index;
                else if (source[index] == dest[dest_offset + accum])
                    ++accum;
                else if (source[index] == dest[dest_offset])
                    accum = 1;
                else
                    accum = 0;
            return -1;
        }
        internal static bool Equals(byte[] source, int source_offset, int source_size, byte[] dest, int dest_offset, int dest_size)
        {
            if (source == null ||
            dest == null ||
            source_offset < 0 ||
            dest_offset < 0 ||
            source_size <= 0 ||
            dest_size <= 0 ||
            source.Length - source_offset - source_size < 0 ||
            dest.Length - dest_offset - dest_size < 0 ||
            (source.Length - source_offset - source_size) - (dest.Length - dest_offset - dest_size) < 0)
                return false;
            int index = 0;
            while (source[source_offset + index] == dest[dest_offset + index] && ++index < dest_size);
            return index == dest_size;
        }
        internal static string UrlDecode(string source)
        {
            byte[] temp = Encoding.UTF8.GetBytes(source);
            return UrlDecode(temp, 0, temp.Length);
        }
        internal static string UrlDecode(byte[] source, int offset, int size)
        {
            if (source == null || offset < 0 || size <= 0 || source.Length - offset - size < 0)
                return null;
            byte[] copy = new byte[size - offset];
            int copy_index = 0;
            while(offset < size)
            {
                switch(source[offset])
                {
                    case (byte)'+':
                    copy[copy_index] = (byte)' ';
                    break;
                    case (byte)'%':
                    if(size - offset < 2)
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
            return Encoding.UTF8.GetString(copy, 0 , copy_index);
        }
        public static string Parse(byte[] buf, int offset, int size)
        {
            HttpRequest request = new HttpRequest();
            int prev, next;
            int start_boundary = IndexOf(buf, offset, size, CRLF, 0, CRLF.Length);
            if ((prev = IndexOf(buf, offset, start_boundary, 0x20)) == -1 ||
                !Enum.TryParse(Encoding.UTF8.GetString(buf, offset, prev), true, out request.Method) ||
                (next = IndexOf(buf, prev + 1, start_boundary, 0x20)) == -1 ||
                !(request.URL = Encoding.ASCII.GetString(buf, prev + 1, next - prev - 1)).StartsWith("/") ||
                (next = IndexOf(buf, next + 1, start_boundary, BINARY, 6, 11)) == -1 ||
                !Version.TryParse(Encoding.UTF8.GetString(buf, next, start_boundary - next - CRLF.Length), out Version result))
                return null;
            int header_boundary = IndexOf(buf, start_boundary, size, DCRLF, 0, DCRLF.Length);
            if (header_boundary == -1)
            {
                return null;
            }
            // using (StringReader reader = new StringReader(System.Text.Encoding.ASCII.GetString(buf, offset, header_boundary - offset)))
            // {
            //     string[] startLine = reader.ReadLine().Split(' ');
            //     if (startLine.Length != 3 ||
            //     !Enum.TryParse(startLine[0], true, out request.Method) ||
            //     !startLine[1].StartsWith("/") ||
            //     !startLine[2].StartsWith("HTTP/") ||
            //     !Version.TryParse(startLine[2].Substring(5), out request.Protocol))
            //     {
            //         request.Result = ParseResult.InvaildProtocol;
            //         return request;
            //     }
            //     request.URL = startLine[1];
            //     string property;
            //     while ((property = reader.ReadLine()) != null)
            //     {
            //         int colonIndex = property.IndexOf(":");
            //         if (colonIndex == -1)
            //             continue;
            //         string name = property.Substring(0, colonIndex).Trim();
            //         string[] values = property.Substring(colonIndex + 1).Split(';');
            //         request.Header[name] = values[0].Trim();
            //         for (int valueIndex = 0; valueIndex < values.Length; ++valueIndex)
            //         {
            //             int equalIndex = values[valueIndex].IndexOf("=");
            //             if (equalIndex >= 0)
            //                 request.Header[name + ":" + values[valueIndex].Substring(0, equalIndex).Trim()] = values[valueIndex].Substring(equalIndex + 1).Trim();
            //         }
            //     }
            // }
            // int questionIndex = request.URL.IndexOf('?');
            // if (questionIndex > 0)
            // {
            //     foreach (string property in request.URL.Substring(questionIndex + 1).Split('&'))
            //     {
            //         int equalIndex = property.IndexOf("=");
            //         if (equalIndex >= 0)
            //             request.Form[UrlDecode(property.Substring(0, equalIndex)).Trim()] = UrlDecode(property.Substring(equalIndex + 1)).Trim();
            //     }
            //     request.URL = request.URL.Substring(0, questionIndex);
            // }
            // request.URL = UrlDecode(request.URL);
            // if (int.TryParse(request.ContentLength, out int contentLength))
            // {
            //     request.Content = new MemoryStream(new byte[contentLength], 0, contentLength, true, true);
            //     request.Content.Write(buffer, markIndex + 1, size - markIndex - 1);
            //     if (request.Content.Position < request.Content.Length)
            //     {
            //         request.Result = ParseResult.KeepConnect;
            //         return request;
            //     }
            // }
            // switch (request.ContentType)
            // {
            //     case "application/x-www-form-urlencoded":
            //         foreach (string property in Encoding.ASCII.GetString((request.Content as MemoryStream).GetBuffer()).Split('&'))
            //         {
            //             int equalIndex = property.IndexOf("=");
            //             if (equalIndex >= 0)
            //                 request.Form[UrlDecode(property.Substring(0, equalIndex).Trim())] = UrlDecode(property.Substring(equalIndex + 1).Trim());
            //         }
            //         break;
            //     case "multipart/form-data":
            //         if (!request.Header.ContainsKey("Content-Type:boundary"))
            //         {
            //             request.Result = ParseResult.InvaildBoundary;
            //             return request;
            //         }
            //         byte[] data = (request.Content as MemoryStream).GetBuffer();
            //         byte[] boundary = Encoding.ASCII.GetBytes("--" + request.Header["Content-Type:boundary"]);
            //         int headIndex = IndexOf(data, boundary, 0);
            //         int rearIndex = 0;
            //         List<HttpGeneralFormat> multipart = new List<HttpGeneralFormat>();
            //         while (true)
            //         {
            //             HttpGeneralFormat multipartdata = new HttpGeneralFormat();
            //             if (headIndex == -1 || (rearIndex = IndexOf(data, CRLF, headIndex)) == -1)
            //                 break;
            //             using (StringReader reader = new StringReader(Encoding.ASCII.GetString(data, headIndex + 3, rearIndex - headIndex - 3)))
            //             {
            //                 string property = reader.ReadLine();
            //                 int colonIndex = property.IndexOf(":");
            //                 if (colonIndex < 0)
            //                     continue;
            //                 string name = property.Substring(0, colonIndex).Trim();
            //                 string[] values = property.Substring(colonIndex + 1).Split(';');
            //                 multipartdata.Header[name] = values[0].Trim();
            //                 for (int valueIndex = 1; valueIndex < values.Length; ++valueIndex)
            //                 {
            //                     int equalIndex = values[valueIndex].IndexOf("=");
            //                     if (equalIndex >= 0)
            //                         multipartdata.Header[name + ":" + values[valueIndex].Substring(0, equalIndex).Trim()] = values[valueIndex].Substring(equalIndex + 1).Trim();
            //                 }
            //             }
            //             if ((headIndex = IndexOf(data, boundary, rearIndex)) == -1)
            //             {
            //                 request.Result = ParseResult.InvaildBoundary;
            //                 return request;
            //             }
            //             multipartdata.Content = new MemoryStream(headIndex - rearIndex - boundary.Length - 2);
            //             multipartdata.Content.Write(data, rearIndex + 1, headIndex - rearIndex - boundary.Length - 2);
            //             multipart.Add(multipartdata);
            //             continue;
            //         }
            //         request = new HttpMultipartRequest(request, multipart);
            //         break;
            // }
            // return request;


            return null;
        }
    }

}