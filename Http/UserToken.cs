using System;
using System.Net.Sockets;

namespace HyperSocket.Http
{
    internal sealed class UserToken : IDisposable
    {
        internal Socket Socket { get; set; }
        internal DateTime Timeout { get; set; }
        internal HttpRequest Request { get; set; }
        internal HttpResponse Response { get; set; }
        internal int KeepAliveCount;

        public void Dispose()
        {
            Socket.Close();
            Request?.Dispose();
            Response?.Dispose();
        }
    }
}