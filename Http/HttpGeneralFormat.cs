using System;
using System.IO;
using System.Collections.Generic;

namespace HyperSocket.Http
{
    public class HttpGeneralFormat : IDisposable
    {
        public Dictionary<string, string> Header { get; internal set; } = new Dictionary<string, string>();
        public Stream Content { get; internal set; }

        ~HttpGeneralFormat()
        {
            Content?.Close();
        }

        public void Dispose()
        {
            Content?.Dispose();
        }
        // public bool TrySearch(string format, out string value)
        // {
        //     int colonIndex = format.IndexOf(":");
        //     if (colonIndex == -1)
        //     {
        //         if (Header.TryGetValue(format, out value))
        //             return true;
        //     }
        //     else
        //     {
        //         string name = format.Substring(0, colonIndex);
        //         string property = format.Substring(colonIndex + 1);
        //         if (Header.TryGetValue(name, out string inner))
        //         {
        //             Console.WriteLine($"inner = {inner} property = {property}");
        //             int propertyIndex = inner.IndexOf(property);
        //             if (propertyIndex >= 0)
        //             {
        //                 propertyIndex = inner.IndexOf("=", propertyIndex);
        //                 value = inner.Substring(propertyIndex, inner.IndexOf(" ", propertyIndex + 1));
        //                 return true;
        //             }
        //         }
        //     }
        //     value = null;
        //     return false;
        // }
    }
}