using System;
using System.IO;
using System.Collections.Generic;

namespace HyperSocket.Http
{
    public class HttpGeneralFormat : IDisposable
    {
        public Dictionary<string, object> Header { get; internal set; } = new Dictionary<string, object>();
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