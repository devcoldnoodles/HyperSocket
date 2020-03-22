using System;
using System.IO;
using System.Text.RegularExpressions;
using HyperSocket.Http;

public static class Test
{
    public static void Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Routers.Add(new HttpEventHandler
        {
            regex = "^/test",
            Get = (req, res) =>
            {
               res.Send("Hello");
               res.StatusCode = HttpStatusCode.OK;
               return true;
            }
        });

        listener.Routers.Add(new HttpRouter
        {
            regex = "^/",
            description = "root",
            handle = (Request, Response) =>
            {
                switch (Request.Method)
                {
                    case HttpMethod.Get:
                        FileInfo file = new FileInfo($"WebContent/resource{Request.URL}");
                        if (!file.Exists)
                        {
                            if (Request.URL == "/")
                            {
                                Response.StatusCode = HttpStatusCode.TemporaryRedirect;
                                Response.Location = "/dvFinedust.html";
                            }
                            else
                            {
                                Response.StatusCode = HttpStatusCode.NotFound;
                                Response.SendFile("WebContent/resource/error.html");
                            }
                            return true;
                        }
                        Response.StatusCode = HttpStatusCode.OK;
                        Response.SendFile(file.FullName);
                        break;
                }
                return true;
            }
        });
        listener.Start(new HttpOptions
        {
            Port = 8081,
            MaxConnections = 32,
        });
        string CommandFormat = "^(?<Command>\\w+)(\\s+(?<Option>\\w+))?$";
        while (listener.IsRun)
        {
            Match match = Regex.Match(Console.ReadLine(), CommandFormat);
            if (match.Success)
                switch (match.Groups["Command"].Value)
                {
                    case "cls":
                        Console.Clear();
                        break;
                    case "exit":
                        listener.Stop();
                        break;
                    default:
                        Console.WriteLine("Unknown Command");
                        break;
                }
        }
        return;
    }
}