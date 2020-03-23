using System;

namespace HyperSocket.Http
{
    public class HttpOptions
    {
        /* Binding Option */
        public Version Version = new Version(1,1);
        public int Port = 80; // Server Address
        public int BufferSize = 1048576;
        public int BacklogSize = 256;
        public int MaxConnections = 256;
        /* Keep Alive Option */
        public bool KeepAlive = true;
        public int KeepAliveMaxRequest = 50;
        public int KeepAliveTimeout = 30;
        /* External Option */
        public string ServerName = "CSP";
        public bool ErrorLog = false;
    }
}