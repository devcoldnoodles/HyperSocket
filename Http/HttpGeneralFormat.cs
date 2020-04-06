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
    }
}