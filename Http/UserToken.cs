using System;
using System.Net.Sockets;

namespace HyperSocket.Http
{
    internal sealed class UserToken : IDisposable
    {
        public Socket Socket;
        public DateTime Timeout;
        public HttpRequest Request;
        public HttpResponse Response;
        public int KeepAliveCount;
        public int State;

        public void Dispose()
        {
            Socket.Close();
            Request?.Dispose();
            Response?.Dispose();
        }
    }
}