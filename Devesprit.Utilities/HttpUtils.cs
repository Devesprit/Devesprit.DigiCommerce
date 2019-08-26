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
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                using (StringWriter sw = new StringWriter())
                {
                    ConvertTo(doc.DocumentNode, sw, new PreceedingDomTextInfo(false));
                    sw.Flush();
                    var formatted = Regex.Replace(sw.ToString(), @"(\r?\n){2,}", "\r\n");
                    return System.Net.WebUtility.HtmlDecode(formatted);
                }
            }
            catch
            {
                return HtmlToPlainText(html);
            }
        }

        internal static string HtmlToPlainText(string html)
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

        internal static void ConvertContentTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText, textInfo);
            }
        }

        internal static void ConvertTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;
                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText, textInfo);
                    break;
                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                    {
                        break;
                    }
                    // get text
                    html = ((HtmlTextNode)node).Text;
                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                    {
                        break;
                    }
                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Length == 0)
                    {
                        break;
                    }
                    if (!textInfo.WritePrecedingWhiteSpace || textInfo.LastCharWasSpace)
                    {
                        html = html.TrimStart();
                        if (html.Length == 0) { break; }
                        textInfo.IsFirstTextOfDocWritten.Value = textInfo.WritePrecedingWhiteSpace = true;
                    }
                    outText.Write(HtmlEntity.DeEntitize(Regex.Replace(html.TrimEnd(), @"\s{2,}", " ")));
                    if (textInfo.LastCharWasSpace = char.IsWhiteSpace(html[html.Length - 1]))
                    {
                        outText.Write(' ');
                    }
                    break;
                case HtmlNodeType.Element:
                    string endElementString = null;
                    bool isInline;
                    bool skip = false;
                    int listIndex = 0;
                    switch (node.Name)
                    {
                        case "nav":
                            skip = true;
                            isInline = false;
                            break;
                        case "body":
                        case "section":
                        case "article":
                        case "aside":
                        case "h1":
                        case "h2":
                        case "header":
                        case "footer":
                        case "address":
                        case "main":
                        case "div":
                        case "p": // stylistic - adjust as you tend to use
                            if (textInfo.IsFirstTextOfDocWritten)
                            {
                                outText.Write("\r\n");
                            }
                            endElementString = "\r\n";
                            isInline = false;
                            break;
                        case "br":
                            outText.Write("\r\n");
                            skip = true;
                            textInfo.WritePrecedingWhiteSpace = false;
                            isInline = true;
                            break;
                        case "a":
                            if (node.Attributes.Contains("href"))
                            {
                                string href = node.Attributes["href"].Value.Trim();
                                if (node.InnerText.IndexOf(href, StringComparison.InvariantCultureIgnoreCase) == -1)
                                {
                                    endElementString = "<" + href + ">";
                                }
                            }
                            isInline = true;
                            break;
                        case "li":
                            if (textInfo.ListIndex > 0)
                            {
                                outText.Write("\r\n{0}.  ", textInfo.ListIndex++);
                            }
                            else
                            {
                                outText.Write("\r\n*  "); //using '*' as bullet char, with double space after, but whatever you want eg "\t->", if utf-8 0x2022
                            }
                            isInline = false;
                            break;
                        case "ol":
                            listIndex = 1;
                            goto case "ul";
                        case "ul": //not handling nested lists any differently at this stage - that is getting close to rendering problems
                            endElementString = "\r\n";
                            isInline = false;
                            break;
                        case "img": //inline-block in reality
                            if (node.Attributes.Contains("alt"))
                            {
                                outText.Write('[' + node.Attributes["alt"].Value);
                                endElementString = "]";
                            }
                            if (node.Attributes.Contains("src"))
                            {
                                outText.Write('<' + node.Attributes["src"].Value + '>');
                            }
                            isInline = true;
                            break;
                        default:
                            isInline = true;
                            break;
                    }
                    if (!skip && node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText, isInline ? textInfo : new PreceedingDomTextInfo(textInfo.IsFirstTextOfDocWritten) { ListIndex = listIndex });
                    }
                    if (endElementString != null)
                    {
                        outText.Write(endElementString);
                    }
                    break;
            }
        }

        internal class PreceedingDomTextInfo
        {
            public PreceedingDomTextInfo(BoolWrapper isFirstTextOfDocWritten)
            {
                IsFirstTextOfDocWritten = isFirstTextOfDocWritten;
            }
            public bool WritePrecedingWhiteSpace { get; set; }
            public bool LastCharWasSpace { get; set; }
            public readonly BoolWrapper IsFirstTextOfDocWritten;
            public int ListIndex { get; set; }
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
