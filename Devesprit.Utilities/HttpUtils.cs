using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace Devesprit.Utilities
{
    public static partial class HttpUtils
    {
        public static string ByteArrayToBase64Image(this byte[] bytes)
        {
            if (bytes == null)
            {
                return "";
            }
            return "data:image/jpeg;base64," + Convert.ToBase64String(bytes, 0, bytes.Length);
        }

        public static string BuildQueryStringUrl(this string url, string[] newQueryStringArr)
        {
            string plainUrl;
            var queryString = string.Empty;

            var newQueryString = string.Join("&", newQueryStringArr);

            if (url.Contains("?"))
            {
                var index = url.IndexOf('?');
                plainUrl = url.Substring(0, index); //URL With No QueryString
                queryString = url.Substring(index + 1);
            }
            else
            {
                plainUrl = url;
            }

            var nvc = HttpUtility.ParseQueryString(queryString);
            var qscoll = HttpUtility.ParseQueryString(newQueryString);

            var queryData = string.Join("&",
                nvc.AllKeys.Where(key =>
                    !string.IsNullOrWhiteSpace(key) && !qscoll.AllKeys.Any(newKey => newKey.Contains(key))).Select(
                    key => string.Format("{0}={1}",
                        HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))).ToArray());
            //Fetch Existing QueryString Except New QueryString

            var delimiter = nvc.HasKeys() && !string.IsNullOrEmpty(queryData) ? "&" : string.Empty;
            var queryStringToAppend = "?" + newQueryString + delimiter + queryData;

            return plainUrl + queryStringToAppend;
        }

        public static string RemoveEmptyParametersFromQueryString(this string url)
        {
            string plainUrl;
            var queryString = string.Empty;

            if (url.Contains("?"))
            {
                var index = url.IndexOf('?');
                plainUrl = url.Substring(0, index); //URL With No QueryString
                queryString = url.Substring(index + 1);
            }
            else
            {
                plainUrl = url;
            }

            var nvc = HttpUtility.ParseQueryString(queryString);

            var queryData = string.Join("&",
                nvc.AllKeys.Where(key => !string.IsNullOrWhiteSpace(nvc[key])).
                    Select(key => string.Format("{0}={1}",
                        HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))).ToArray());
            
            var queryStringToAppend = !string.IsNullOrEmpty(queryData) ? "?" + queryData : "";

            return plainUrl + queryStringToAppend;
        }

        public static string ConvertHtmlToText(this string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (doc.ParseErrors.Any())
            {
                return HtmlToPlainText(html);
            }
            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        private static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode) node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }

                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "br":
                            outText.Write("\r\n");
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }

                    break;
            }
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        internal class BoolWrapper
        {
            public BoolWrapper() { }
            public bool Value { get; set; }
            public static implicit operator bool(BoolWrapper boolWrapper)
            {
                return boolWrapper.Value;
            }
            public static implicit operator BoolWrapper(bool boolWrapper)
            {
                return new BoolWrapper { Value = boolWrapper };
            }
        }
    }
}
