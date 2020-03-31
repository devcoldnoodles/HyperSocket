using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace HyperSocket.Http
{
    public sealed class HttpListener
    {
        private static readonly byte[] CRLF = { 0x0D, 0x0A };
        private static readonly byte[] DCRLF = { 0x0D, 0x0A, 0x0D, 0x0A };

        private Socket socket;
        private ConcurrentQueue<SocketAsyncEventArgs> handlers = new ConcurrentQueue<SocketAsyncEventArgs>();
        private HttpOptions options;
        private List<HttpRouter> routers = new List<HttpRouter>();

        public bool IsRunning { get; private set; } = false;
        public List<HttpRouter> Routers { get { return routers; } }

        public void Start(HttpOptions value = null)
        {
            if (value == null) options = new HttpOptions();
            else options = value;
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
                        default:
                            //throw new ArgumentException("Undefined Operation");
                            break;
                    }
                });
                handler.SetBuffer(new byte[options.BufferSize], 0, options.BufferSize);
                handlers.Enqueue(handler);
            }
            //source = new CancellationTokenSource();
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
            //source?.Cancel();
            IsRunning = false;
            socket?.Close();
        }
        private static int IndexOf(byte[] source, byte[] dest, int offset = 0)
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
        private static string UrlDecode(byte[] source, int offset, int size)
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
        private static string UrlDecode(string source)
        {
            byte[] temp = Encoding.UTF8.GetBytes(source);
            return UrlDecode(temp, 0, temp.Length);
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
            UserToken client = (UserToken)e.UserToken;
            HttpRequest Request = client.Request;
            HttpResponse Response = client.Response;
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                ErrorHandle(client, e);
                return;
            }
            if (e.Count != options.BufferSize)
                e.SetBuffer(e.Offset, options.BufferSize);
            Console.WriteLine(Encoding.ASCII.GetString(e.Buffer, e.Offset, e.BytesTransferred));
            //HttpParser.Parse(e.Buffer, e.Offset, e.BytesTransferred);
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    break;
                case SocketAsyncOperation.Send:
                    break;
            }
            if (client.Request != null && client.Response.Content != null)
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
                Request = client.Request = new HttpRequest();
                int markIndex = IndexOf(e.Buffer, DCRLF, e.Offset);
                if (markIndex == -1)
                {
                    Debug.Print("Invalid Format");
                    ErrorHandle(client, e);
                    return;
                }
                using (StringReader reader = new StringReader(Encoding.ASCII.GetString(e.Buffer, e.Offset, markIndex - e.Offset - DCRLF.Length)))
                {
                    string[] startLine = reader.ReadLine().Split(' ');
                    if (startLine.Length != 3 ||
                    !Enum.TryParse(startLine[0], true, out Request.Method) ||
                    !(Request.URL = startLine[1]).StartsWith("/") ||
                    !startLine[2].StartsWith("HTTP/") ||
                    !Version.TryParse(startLine[2].Substring(5), out Request.Protocol))
                    {
                        Debug.Print("invalid http protocol");
                        ErrorHandle(client, e);
                        return;
                    }
                    string property;
                    while ((property = reader.ReadLine()) != null)
                    {
                        int colonIndex = property.IndexOf(":");
                        if (colonIndex == -1)
                        {
                            Debug.Print("Invalid Header Format");
                            ErrorHandle(client, e);
                            return;
                        }
                        Request.Header[property.Substring(0, colonIndex)] = property.Substring(colonIndex + 1);
                    }
                }
                if (int.TryParse(Request.ContentLength, out int contentLength))
                {
                    Request.Content = new MemoryStream(new byte[contentLength], 0, contentLength, true, true);
                    Request.Content.Write(e.Buffer, markIndex, e.BytesTransferred - markIndex);
                    if (Request.Content.Position < Request.Content.Length)
                    {
                        if (!client.Socket.ReceiveAsync(e))
                            ReceiveProcess(client.Socket, e);
                        return;
                    }
                }
            }
            switch (Request.Method)
            {
                case HttpMethod.Get:
                    int questionIndex = Request.URL.IndexOf('?');
                    if (questionIndex > 0)
                    {
                        var dataset = new Dictionary<string, string>();
                        foreach (string property in Request.URL.Substring(questionIndex + 1).Split('&'))
                        {
                            int equalIndex = property.IndexOf("=");
                            if (equalIndex >= 0)
                                dataset[UrlDecode(property.Substring(0, equalIndex)).Trim()] = UrlDecode(property.Substring(equalIndex + 1)).Trim();
                        }
                        Request.dataset = dataset;
                        Request.URL = Request.URL.Substring(0, questionIndex);
                    }
                    Request.URL = UrlDecode(Request.URL);
                    break;
                case HttpMethod.Post:
                    switch (Request.ContentType)
                    {
                        case "application/x-www-form-urlencoded":
                            var dataset = new Dictionary<string, string>();
                            foreach (string property in Encoding.ASCII.GetString((Request.Content as MemoryStream).GetBuffer()).Split('&'))
                            {
                                int equalIndex = property.IndexOf("=");
                                if (equalIndex >= 0)
                                    dataset[UrlDecode(property.Substring(0, equalIndex).Trim())] = UrlDecode(property.Substring(equalIndex + 1).Trim());
                            }
                            Request.dataset = dataset;
                            break;
                        case "multipart/form-data":
                            if (!Request.Header.ContainsKey("Content-Type:boundary"))
                            {
                                Debug.Print("Invalid boundary");
                                ErrorHandle(client, e);
                                return;
                            }
                            byte[] data = (Request.Content as MemoryStream).GetBuffer();
                            byte[] boundary = Encoding.ASCII.GetBytes("--" + Request.Header["Content-Type:boundary"]);
                            int headIndex = IndexOf(data, boundary, 0);
                            int rearIndex = 0;
                            List<HttpGeneralFormat> multipart = new List<HttpGeneralFormat>();
                            while (true)
                            {
                                HttpGeneralFormat multipartdata = new HttpGeneralFormat();
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
                                        string name = property.Substring(0, colonIndex).Trim();
                                        string[] values = property.Substring(colonIndex + 1).Split(';');
                                        multipartdata.Header[name] = values[0].Trim();
                                        for (int valueIndex = 1; valueIndex < values.Length; ++valueIndex)
                                        {
                                            int equalIndex = values[valueIndex].IndexOf("=");
                                            if (equalIndex >= 0)
                                                multipartdata.Header[name + ":" + values[valueIndex].Substring(0, equalIndex).Trim()] = values[valueIndex].Substring(equalIndex + 1).Trim();
                                        }
                                    }
                                }
                                if ((headIndex = IndexOf(data, boundary, rearIndex)) == -1)
                                {
                                    Debug.Print("Invalid boundary");
                                    ErrorHandle(client, e);
                                    return;
                                }
                                multipartdata.Content = new MemoryStream(headIndex - rearIndex - boundary.Length - CRLF.Length);
                                multipartdata.Content.Write(data, rearIndex, headIndex - rearIndex - boundary.Length - CRLF.Length);
                                multipart.Add(multipartdata);
                                continue;
                            }
                            Request.dataset = multipart;
                            break;
                    }
                    break;
            }
            foreach (var router in routers)
            {
                if (Regex.IsMatch(Request.URL, router.regex))
                {
                    try
                    {
                        router.handle(Request, Response);
                    }
                    catch (Exception exception)
                    {
                        Debug.Print(exception.ToString());
                        ErrorHandle(client, e);
                        return;
                    }
                }
            }
            if (options.KeepAlive && Request.Connection.ToLower() == "keep-alive" && client.KeepAliveCount >= 0)
            {
                client.Timeout = DateTime.Now.AddSeconds(options.KeepAliveTimeout);
                Response.Connection = "keep-alive";
                Response.KeepAlive = $"timeout={options.KeepAliveTimeout},max={--client.KeepAliveCount}";
            }
            string output = Response.ToString();
            e.SetBuffer(e.Offset, Encoding.UTF8.GetBytes(output, 0, output.Length, e.Buffer, e.Offset));
            e.UserToken = client;
            if (!client.Socket.SendAsync(e))
                SendProcess(client.Socket, e);
        }
        private void SendProcess(object sender, SocketAsyncEventArgs e)
        {
            UserToken client = (UserToken)e.UserToken;
            if (e.SocketError != SocketError.Success)
            {
                ErrorHandle(client, e);
                return;
            }
            if (client.Response.Content != null)
            {
                if (client.Response.Content.Position >= client.Response.Content.Length)
                {
                    client.Response.Content.Close();
                    client.Response.Content = null;
                    goto WaitRecv;
                }
                e.SetBuffer(e.Offset, client.Response.Content.Read(e.Buffer, e.Offset, e.Buffer.Length));
                if (!client.Socket.SendAsync(e))
                    SendProcess(sender, e);
                return;
            }
        WaitRecv:
            if (client.Request.Connection.ToLower() == "close")
                ErrorHandle(client, e);
            else if (!client.Socket.ReceiveAsync(e))
                ReceiveProcess(client.Socket, e);
        }
        private void ErrorHandle(UserToken client, SocketAsyncEventArgs e)
        {
            client.Dispose();
            client.Socket.Close();
            handlers.Enqueue(e);
        }
    }
}