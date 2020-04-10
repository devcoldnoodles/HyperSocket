using System;
using System.Collections.Generic;

namespace HyperSocket.Http
{
    public abstract class HttpGeneralHeaderFormat
    {
        public string Value { get; internal set; }
        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public HttpGeneralHeaderFormat(string value)
        {
            string[] values = value.Split(';');
            Value = values[0];
            for (int index = 1; index < values.Length; ++index)
            {
                int equalIndex = values[index].IndexOf("=");
                if (equalIndex == -1)
                    throw new Exception("Invalid Format");
                Parameters[values[index].Substring(0, equalIndex)] = values[index].Substring(equalIndex + 1);
            }
        }

        public override string ToString()
        {
            string temp = Value;
            foreach (var param in Parameters)
                temp += $";{param.Key}:{param.Value}";
            return temp;
        }
    }

    public class ContentType : HttpGeneralHeaderFormat
    {
        public ContentType(string value) : base(value) {}
        public string Boundary { get { return Parameters["boundary"]; } set { Parameters["boundary"] = value; } }
        public string CharSet { get { return Parameters["charset"]; } set { Parameters["charset"] = value; } }
        public string Name { get { return Parameters["name"]; } set { Parameters["name"] = value; } }
    }

    public class ContentDisposition : HttpGeneralHeaderFormat
    {
        public ContentDisposition(string value) : base(value) {}
        public string FileName { get { return Parameters["filename"]; } set { Parameters["filename"] = value; } }
        public string Name { get { return Parameters["name"]; } set { Parameters["name"] = value; } }
    }

}