using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Devesprit.Core.Settings;

namespace Devesprit.Utilities.Extensions
{
    public static partial class StringExtensions
    {
        public static string TruncateText(this string text, int maxLength)
        {
            maxLength = maxLength <= 0 ? int.MaxValue : maxLength;
            text = text.Trim();
            if (string.IsNullOrWhiteSpace(text) || text.Length < maxLength || text.IndexOf(" ", maxLength, StringComparison.Ordinal) == -1)
                return text;

            var iNextSpace = text.LastIndexOf(" ", maxLength, StringComparison.Ordinal);
            return text.Substring(0, iNextSpace > 0 ? iNextSpace : maxLength).Trim() + " ...";
        }

        public static bool IsValidUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _);
        }

        public static bool IsRtlLanguage(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            var strRegex = "\\p{IsArabic}|\\p{IsHebrew}";
            var regex = new Regex(strRegex);
            if (regex.IsMatch(text))
            {
                return true; //first character is RTL
            }

            var count = regex.Matches(text).Count;
            var rtlCharsPercent = ((100 * count) / text.Length);
            if (rtlCharsPercent > 40) //if string contains more than 40% RTL characters
            {
                return true;
            }

            return false;
        }

        public static string EncryptString(this string inputText, string key = "", string salt = "")
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(salt))
            {
                key = ConfigurationManager.AppSettings["EncryptionKey"];
                salt = ConfigurationManager.AppSettings["EncryptionSalt"];
            }
            byte[] plainText = Encoding.UTF8.GetBytes(inputText);

            using (RijndaelManaged rijndaelCipher = new RijndaelManaged())
            {
                PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(salt));
                using (ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainText, 0, plainText.Length);
                            cryptoStream.FlushFinalBlock();
                            string base64 = Convert.ToBase64String(memoryStream.ToArray());
                            return base64;
                        }
                    }
                }
            }
        }

        public static string DecryptString(this string inputText, string key = "", string salt = "")
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(salt))
            {
                key = ConfigurationManager.AppSettings["EncryptionKey"];
                salt = ConfigurationManager.AppSettings["EncryptionSalt"];
            }
            byte[] encryptedData = Convert.FromBase64String(inputText);
            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(salt));

            using (RijndaelManaged rijndaelCipher = new RijndaelManaged())
            {
                using (ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                {
                    using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            byte[] plainText = new byte[encryptedData.Length];
                            cryptoStream.Read(plainText, 0, plainText.Length);
                            string utf8 = Encoding.UTF8.GetString(plainText);
                            return utf8.Trim('\0');
                        }
                    }
                }
            }
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string WildCardToRegular(this string pattern)
        {
            return "^" + Regex.Escape(pattern).Replace("#", "(\\d)").Replace("\\?", "(.)").Replace("\\*", "(.*)") + "$";
        }

        public static bool IsMatchWildcard(this string str, string pattern, bool ignoreCase)
        {
            return Regex.IsMatch(str, pattern.WildCardToRegular(), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        public static bool IsAbsoluteUrl(this string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public static string GetAbsoluteUrl(this string url, Uri baseUri)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }
            if (url.IsAbsoluteUrl())
            {
                return url;
            }

            return new System.Uri(baseUri, url).AbsoluteUri;
        }

        public static string GetAbsoluteUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }
            if (url.IsAbsoluteUrl())
            {
                return url;
            }

            if (HttpContext.Current != null)
            {
                return GetAbsoluteUrl(url, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)));
            }

            var siteUrl = DependencyResolver.Current.GetService<ISettingService>().FindByKey("SiteUrl", "");
            return GetAbsoluteUrl(url, new Uri(siteUrl));
        }

        public static string TrimStart(this string target, string trimString, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            string result = target;
            while (result.StartsWith(trimString, comparison))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string TrimEnd(this string target, string trimString, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            string result = target;
            while (result.EndsWith(trimString, comparison))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

        public static string PrettyXml(this string xml)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                // Handle the exception
            }

            mStream.Close();
            writer.Close();

            return result;
        }

        public static string NormalizeUrl(this string url, int maxLength = 0)
        {
            // Return empty value if text is null
            if (url == null) return "";

            var text = url.Replace(";", "")
                .Replace("?", "")
                .Replace("/", "")
                .Replace(":", "")
                .Replace("@", "")
                .Replace("=", "")
                .Replace("&", "")
                .Replace(" ", "_")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("\"", "")
                .Replace("%", "")
                .Replace("#", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("|", "")
                .Replace("\\", "")
                .Replace("^", "")
                .Replace("~", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("`", "")
                .Replace("!", "")
                .Replace("$", "")
                .Replace("*", "");

            text = Regex.Replace(text, "_+", "_");

            var normalizedString = text
                // Make lowercase
                .ToLowerInvariant()
                // Normalize the text
                .Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            var stringLength = normalizedString.Length;
            var prevdash = false;
            var trueLength = 0;
            char c;
            for (int i = 0; i < stringLength; i++)
            {
                c = normalizedString[i];
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    // Check if the character is a letter or a digit if the character is a
                    // international character remap it to an ascii valid character
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (c < 128)
                            stringBuilder.Append(c);
                        else
                            stringBuilder.Append(RemapInternationalCharToAscii(c));
                        prevdash = false;
                        trueLength = stringBuilder.Length;
                        break;
                    // Check if the character is to be replaced by a hyphen but only if the last character wasn't
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.OtherPunctuation:
                    case UnicodeCategory.MathSymbol:
                        if (!prevdash)
                        {
                            stringBuilder.Append('-');
                            prevdash = true;
                            trueLength = stringBuilder.Length;
                        }
                        break;
                }
                // If we are at max length, stop parsing
                if (maxLength > 0 && trueLength >= maxLength)
                    break;
            }
            // Trim excess hyphens
            var result = stringBuilder.ToString().Trim('-');
            // Remove any excess character to meet maxlength criteria
            return maxLength <= 0 || result.Length <= maxLength ? result : result.Substring(0, maxLength);
        }

        /// <summary>
        /// Remaps international characters to ascii compatible ones
        /// based of: https://meta.stackexchange.com/questions/7435/non-us-ascii-characters-dropped-from-full-profile-url/7696#7696
        /// </summary>
        /// <param name="c">Character to remap</param>
        /// <returns>Remapped character</returns>
        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }

        public static bool IsNormalizedUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            return !(url.Contains(";") ||
                url.Contains("?") ||
                url.Contains("/") ||
                url.Contains(":") ||
                url.Contains("@") ||
                url.Contains("=") ||
                url.Contains("&") ||
                url.Contains(" ") ||
                url.Contains("<") ||
                url.Contains(">") ||
                url.Contains("\"") ||
                url.Contains("%") ||
                url.Contains("#") ||
                url.Contains("{") ||
                url.Contains("}") ||
                url.Contains("|") ||
                url.Contains("\\") ||
                url.Contains("^") ||
                url.Contains("~") ||
                url.Contains("[") ||
                url.Contains("]") ||
                url.Contains("`") ||
                url.Contains("!") ||
                url.Contains("$") ||
                url.Contains("*"));
        }

        public static string ReplaceFirstOccurence(this string originalValue, string occurenceValue, string newValue, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (string.IsNullOrEmpty(originalValue))
                return string.Empty;
            if (string.IsNullOrEmpty(occurenceValue))
                return originalValue;
            if (string.IsNullOrEmpty(newValue))
                return originalValue;
            int startIndex = originalValue.IndexOf(occurenceValue, comparison);
            return originalValue.Remove(startIndex, occurenceValue.Length).Insert(startIndex, newValue);
        }
        
        public static string ReplaceLastOccurence(this string originalValue, string occurenceValue, string newValue, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (string.IsNullOrEmpty(originalValue))
                return string.Empty;
            if (string.IsNullOrEmpty(occurenceValue))
                return originalValue;
            if (string.IsNullOrEmpty(newValue))
                return originalValue;
            int startIndex = originalValue.LastIndexOf(occurenceValue, comparison);
            return originalValue.Remove(startIndex, occurenceValue.Length).Insert(startIndex, newValue);
        }
    }
}
