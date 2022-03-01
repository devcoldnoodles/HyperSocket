using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using static HyperSocket.Http.Utility;

namespace HyperSocket.Http
{
    public sealed class HttpListener
    {
        private Socket socket;
        private ConcurrentQueue<SocketAsyncEventArgs> handlers = new ConcurrentQueue<SocketAsyncEventArgs>();
        private HttpOptions options;
        private List<HttpRouter> routers = new List<HttpRouter>();

        public bool IsRunning { get; private set; } = false;
        public List<HttpRouter> Routers { get { return routers; } }

        public void Start(HttpOptions value = null)
        {
            if (value == null)  options = new HttpOptions();
            else                options = value;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, options.Port));
            }
            catch (SocketException e)
            {
                throw e;
            }
            for (int index = 0; index < options.MaxConnections; ++index)
            {
                SocketAsyncEventArgs handler = new SocketAsyncEventArgs();
                handler.Completed += new EventHandler<SocketAsyncEventArgs>((sender, e) =>
                {
                    switch (e.LastOperation)
                    {
                        case SocketAsyncOperation.Receive:
                            ReceiveProcess(sender, e);
                            break;
                        case SocketAsyncOperation.Send:
                            SendProcess(sender, e);
                            break;
                    }
                });
                handler.SetBuffer(new byte[options.BufferSize], 0, options.BufferSize);
                handlers.Enqueue(handler);
            }
            IsRunning = true;
            socket.Listen(options.BacklogSize);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptProcess);
            //args.SetBuffer(new byte[options.BufferSize], 0, options.BufferSize);
            if (!socket.AcceptAsync(args))
                AcceptProcess(socket, args);
        }
        public void Stop()
        {
            IsRunning = false;
            socket?.Close();
        }
        private void AcceptProcess(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                return;

            if (handlers.TryDequeue(out SocketAsyncEventArgs handler))
            {
                handler.UserToken = new UserToken
                {
                    Socket = e.AcceptSocket,
                    Timeout = DateTime.Now.AddSeconds(options.KeepAlive ? options.KeepAliveTimeout : 0),
                    KeepAliveCount = options.KeepAlive ? options.KeepAliveMaxRequest : 1,
                    Response = new HttpResponse
                    {
                        Protocol = new Version(1, 1),
                        Connection = "close",
                        StatusCode = HttpStatusCode.NotFound,
                        Server = options.ServerName,
                        ContentLength = "0",
                        //Content = new MemoryStream(0),
                    }
                };

                if (!e.AcceptSocket.ReceiveAsync(handler))
                    ReceiveProcess(e.AcceptSocket, handler);
            }
            else
            {
                string output = "HTTP/1.1 503\r\nConnection:close\r\n\r\n";
                e.SetBuffer(e.Offset, Encoding.UTF8.GetBytes(output, 0, output.Length, e.Buffer, e.Offset));
                if (!e.AcceptSocket.SendAsync(e))
                    AcceptProcess(sender, e);
            }
            e.AcceptSocket = null;
            if (!socket.AcceptAsync(e))
                AcceptProcess(sender, e);
        }
        private void ReceiveProcess(object sender, SocketAsyncEventArgs e)
        {
            UserToken client = e.UserToken as UserToken;
            if (client == null)
                return;

            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                ErrorHandle(client, e);
                return;
            }

            if (e.Count != options.BufferSize)
                e.SetBuffer(e.Offset, options.BufferSize);

            Debug.Print(Encoding.ASCII.GetString(e.Buffer, e.Offset, e.BytesTransferred));

            if (client.Request?.Content != null)
            {
                client.Request.Content.Write(e.Buffer, e.Offset, e.BytesTransferred);
                if (client.Request.Content.Position < client.Request.Content.Length)
                {
                    if (!client.Socket.ReceiveAsync(e))
                        ReceiveProcess(sender, e);
                    return;
                }
            }
            else
            {
                client.Request = new HttpRequest();
                int markIndex = IndexOf(e.Buffer, DCRLF, e.Offset);
                if (markIndex == -1)
                {
                    ErrorHandle(client, e, "Invalid Format");
                    return;
                }

                using (StringReader reader = new StringReader(Encoding.ASCII.GetString(e.Buffer, e.Offset, markIndex - e.Offset - DCRLF.Length)))
                {
                    string[] startLine = reader.ReadLine().Split(' ');
                    if (startLine.Length != 3 ||
                    !Enum.TryParse(startLine[0], true, out client.Request.Method) ||
                    !(client.Request.URL = startLine[1]).StartsWith("/") ||
                    !startLine[2].StartsWith("HTTP/") ||
                    !Version.TryParse(startLine[2].Substring(5), out client.Request.Protocol))
                    {
                        ErrorHandle(client, e, "Invalid http protocol");
                        return;
                    }
                    string property;
                    while ((property = reader.ReadLine()) != null)
                    {
                        int colonIndex = property.IndexOf(":");
                        if (colonIndex == -1)
                        {
                            ErrorHandle(client, e, "Invalid Header Format");
                            return;
                        }
                        client.Request.Header[property.Substring(0, colonIndex)] = property.Substring(colonIndex + 1);
                    }
                }

                if (int.TryParse(client.Request.ContentLength, out int contentLength))
                {
                    client.Request.Content = new MemoryStream(new byte[contentLength], 0, contentLength, true, true);
                    client.Request.Content.Write(e.Buffer, markIndex, e.BytesTransferred - markIndex);
                    if (client.Request.Content.Position < client.Request.Content.Length)
                    {
                        if (!client.Socket.ReceiveAsync(e))
                            ReceiveProcess(client.Socket, e);
                        return;
                    }
                }
            }

            foreach (var router in routers)
            {
                if ((client.Request.Match = Regex.Match(client.Request.URL, router.Pattern)).Success)
                {
                    try
                    {
                        router.Handle(client.Request, client.Response);
                    }
                    catch (Exception exception)
                    {
                        Debug.Print(exception.ToString());
                        ErrorHandle(client, e);
                        return;
                    }
                    break;
                }
            }

            if (options.KeepAlive && client.Request.Connection.ToLower() == "keep-alive" && client.KeepAliveCount >= 0)
            {
                client.Timeout = DateTime.Now.AddSeconds(options.KeepAliveTimeout);
                client.Response.Connection = "keep-alive";
                client.Response.KeepAlive = $"timeout={options.KeepAliveTimeout},max={--client.KeepAliveCount}";
            }
            
            string output = client.Response.ToString();
            e.SetBuffer(e.Offset, Encoding.UTF8.GetBytes(output, 0, output.Length, e.Buffer, e.Offset));
            e.UserToken = client;
            if (!client.Socket.SendAsync(e))
                SendProcess(client.Socket, e);
        }
        
        private void SendProcess(object sender, SocketAsyncEventArgs e)
        {
            UserToken client = e.UserToken as UserToken;
            if (client == null)
                return;

            if (e.SocketError != SocketError.Success)
            {
                ErrorHandle(client, e);
                return;
            }

            if (client.Response?.Content != null)
            {
                do
                {
                    if (client.Response.Content.Position >= client.Response.Content.Length)
                    {
                        client.Response.Content.Close();
                        client.Response.Content = null;
                        goto WaitRecv;
                    }

                    e.SetBuffer(e.Offset, client.Response.Content.Read(e.Buffer, e.Offset, e.Buffer.Length));
                } while (!client.Socket.SendAsync(e));
                return;
            }
        WaitRecv:
            if (client.Request.Connection.ToLower() == "close")
                ErrorHandle(client, e);
            else if (!client.Socket.ReceiveAsync(e))
                ReceiveProcess(client.Socket, e);
        }
        private void ErrorHandle(UserToken client, SocketAsyncEventArgs e, string message = null)
        {
            if(message != null)
                Debug.Print(message);

            client?.Dispose();
            client?.Socket?.Close();
            handlers?.Enqueue(e);
        }
    }
}