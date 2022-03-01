using System;
using System.Net.Sockets;

namespace HyperSocket.Http
{
    public sealed class UserToken : IDisposable
    {
        public Socket Socket { get; internal set; }
        public DateTime Timeout { get; internal set; }
        public HttpRequest Request { get; internal set; }
        public HttpResponse Response { get; internal set; }
        public int KeepAliveCount;

        public void Dispose()
        {
            Socket?.Close();
            Request?.Dispose();
            Response?.Dispose();
        }
    }
}