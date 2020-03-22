using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace HyperSocket.Http
{
    public static class CSPExpression
    {
        internal static bool TryParseExpression(string expression, object data, out object result)
        {
            if (TryParseRValueExpression(expression, data, out result))
                return true;
            if (TryParseLValueExpression(expression, data, out result))
                return true;
            result = null;
            return false;
        }
        internal static bool IsExpression(string expression, object data, out string result)
        {
            if (expression.StartsWith("{{") && expression.EndsWith("}}"))
            {
                result = expression.Substring(2, expression.Length - 4);
                return true;
            }
            result = null;
            return false;
        }
        internal static bool TryParseLValueExpression(string expression, object data, out object result)
        {
            result = null;
            return false;
        }
        internal static bool TryParseRValueExpression(string expression, object data, out object result)
        {
            switch (expression.ToLower())
            {
                case "true":
                    result = true;
                    break;
                case "false":
                    result = false;
                    break;
                case "null":
                    result = null;
                    break;
                default:
                    if (int.TryParse(expression, out int integer))
                    {
                        result = integer;
                    }
                    else if (double.TryParse(expression, out double number))
                    {
                        result = number;
                    }
                    else if (expression.StartsWith('\"') && expression.EndsWith('\"'))
                    {
                        result = expression.Substring(1, expression.Length - 2);
                    }
                    else if (expression.StartsWith('\'') && expression.EndsWith('\''))
                    {
                        if (expression.Length != 3)
                            throw new Exception("(\') can only have one character");
                        result = expression[1];
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                    break;
            }
            return true;
        }
        internal static string VarialbeExpression(string source, object data, string optional_source = null, object optional_data = null)
        {
            StringBuilder builder = new StringBuilder(source);
            Match match = Regex.Match(source, "{{(.+?)}}");
            while (match.Success)
            {
                string[] fields = match.Groups[1].Value.Split('.');
                builder.Replace(match.Groups[0].Value, (fields[0] == optional_source ? AttributeAccess(optional_data, fields, 1) : AttributeAccess(data, fields)).ToString());
                match = match.NextMatch();
            }
            return builder.ToString();
        }
        internal static object AttributeAccess(object target, string[] fields, int offset = 0)
        {
            for (int index = offset; index < fields.Length; ++index)
                target = target.GetType().InvokeMember(fields[index], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.InvokeMethod, null, target, null);
            return target;
        }
        public static string Render(string source, object data)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//*[name()='csp:include']") ?? Enumerable.Empty<HtmlNode>())
            {
                HtmlNode store = document.CreateTextNode("");
                store.InnerHtml = File.ReadAllText(node.Attributes["src"].Value.Trim()).Trim();
                node.ParentNode.ReplaceChild(store, node);
            }
            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//*[name()='csp:if']") ?? Enumerable.Empty<HtmlNode>())
            {
                HtmlNode store = document.CreateTextNode("");
                object lexpr = null;
                object rexpr = null;
                if (TryParseRValueExpression(node.Attributes["lexpr"].Value, data, out object reslexpr))
                    lexpr = reslexpr;
                else
                    lexpr = AttributeAccess(data, node.Attributes["lexpr"].Value.Trim().Split('.'));
                if (TryParseRValueExpression(node.Attributes["rexpr"].Value, data, out object resrexpr))
                    rexpr = resrexpr;
                else
                    rexpr = AttributeAccess(data, node.Attributes["rexpr"].Value.Trim().Split('.'));
                if (lexpr?.ToString() == rexpr?.ToString())
                    store.InnerHtml = node.InnerHtml.Trim();
                node.ParentNode.ReplaceChild(store, node);
            }
            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//*[name()='csp:ifn']") ?? Enumerable.Empty<HtmlNode>())
            {
                HtmlNode store = document.CreateTextNode("");
                object lexpr = null;
                object rexpr = null;
                if (TryParseRValueExpression(node.Attributes["lexpr"].Value, data, out object reslexpr))
                    lexpr = reslexpr;
                else
                    lexpr = AttributeAccess(data, node.Attributes["lexpr"].Value.Trim().Split('.'));
                if (TryParseRValueExpression(node.Attributes["rexpr"].Value, data, out object resrexpr))
                    rexpr = resrexpr;
                else
                    rexpr = AttributeAccess(data, node.Attributes["rexpr"].Value.Trim().Split('.'));
                if (lexpr?.ToString() != rexpr?.ToString())
                    store.InnerHtml = node.InnerHtml.Trim();
                node.ParentNode.ReplaceChild(store, node);
            }
            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//*[name()='csp:foreach']") ?? Enumerable.Empty<HtmlNode>())
            {
                StringBuilder temp = new StringBuilder();
                HtmlNode store = document.CreateTextNode("");
                foreach (object item in AttributeAccess(data, node.Attributes["list"].Value.Trim().Split('.')) as IEnumerable)
                    temp.Append(VarialbeExpression(node.InnerHtml.Trim(), data, node.Attributes["item"].Value.Trim(), item));
                store.InnerHtml = temp.ToString().Trim();
                node.ParentNode.ReplaceChild(store, node);
            }
            return VarialbeExpression(document.DocumentNode.InnerHtml, data);
        }
    }
}