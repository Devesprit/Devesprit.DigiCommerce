using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using CommonServiceLocator;
using Devesprit.FileServer.Repository.Interfaces;
using Devesprit.Utilities.Extensions;
using Elmah;

namespace Devesprit.FileServer
{
    public partial class Download : HttpTaskAsyncHandler, IRequiresSessionState
    {
        private IFileManagerRepository _fileManagerRepository;
        public IFileManagerRepository FileManagerRepository => _fileManagerRepository ??
                                                               (_fileManagerRepository =
                                                                   ServiceLocator.Current
                                                                       .GetInstance<IFileManagerRepository>());
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            if (IsCrawlByBot(context.Request))
            {
                ReturnHttpStatusCode(context, HttpStatusCode.OK, "Bots detected.");
                return;
            }

            var request = context.Request.QueryString["request"];
            if (string.IsNullOrWhiteSpace(request))
            {
                ReturnHttpStatusCode(context, HttpStatusCode.BadRequest, "Malformed request syntax.");
                return;
            }

            string filePath;
            int downloadLimitCount;
            DateTime expireDate;

            try
            {
                var requestObject = request.DecryptString().JsonToObject<DownloadRequest>();
                filePath = requestObject.File;
                expireDate = requestObject.Expire;
                downloadLimitCount = requestObject.DownloadCount;
            }
            catch(Exception e)
            {
                ReturnHttpStatusCode(context, HttpStatusCode.BadRequest, "Malformed request syntax.");
                Elmah.ErrorLog.GetDefault(context).Log(new Error(e));
                return;
            }

            if (!await Task.Run(() => File.Exists(filePath)))
            {
                ReturnHttpStatusCode(context, HttpStatusCode.NotFound, "The requested resource was not found.");
                return;
            }

            if (expireDate > DateTime.MinValue && expireDate < DateTime.Now)
            {
                ReturnHttpStatusCode(context, HttpStatusCode.Forbidden, "The requested resource was expired.");
                return;
            }

            if (downloadLimitCount > 0)
            {
                var dlCount = await FileManagerRepository.GetDownloadCount(request);
                if (dlCount >= downloadLimitCount)
                {
                    ReturnHttpStatusCode(context, HttpStatusCode.Forbidden, "The requested resource was expired.");
                    return;
                }
            }

            if (await Task.Run(() => TransmitFile(context, filePath, File.GetLastWriteTimeUtc(filePath).ToString("O"))) &&
                downloadLimitCount > 0)
                await FileManagerRepository.LogDownloadRequest(filePath, request, context);
        }

        protected virtual void ReturnHttpStatusCode(HttpContext context, HttpStatusCode statusCode, string error)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "text/plain";
            context.Response.Write(error);
            context.Response.End();
        }

        /// <summary>
        /// Writes the file stored in the file system to the response stream without buffering in memory, ideal for large files. Supports resumable downloads.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filename">The name of the file to write to the HTTP output.</param>
        /// <param name="etag">A unique identifier for the content. Required for IE9 resumable downloads, must be a strong etag which means begins and ends in a quote i.e. "\"6c132-941-ad7e3080\""</param>
        protected virtual bool TransmitFile(HttpContext context, string filename, string etag)
        {
            var request = context.Request;
            var response = context.Response;

            var fileInfo = new FileInfo(filename);
            var responseLength = fileInfo.Exists ? fileInfo.Length : 0;
            long startIndex = 0;

            //if the "If-Match" exists and is different to etag (or is equal to any "*" with no resource) then return 412 precondition failed
            if (request.Headers["If-Match"] == "*" && !fileInfo.Exists ||
                request.Headers["If-Match"] != null && request.Headers["If-Match"] != "*" &&
                request.Headers["If-Match"] != etag)
            {
                ReturnHttpStatusCode(context, HttpStatusCode.PreconditionFailed, "");
                return false;
            }

            if (request.Headers["If-None-Match"] == etag)
            {
                ReturnHttpStatusCode(context, HttpStatusCode.NotModified, "");
                return false;
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            if (request.Headers["Range"] != null && (request.Headers["If-Range"] == null || request.Headers["IF-Range"] == etag))
            {
                var match = Regex.Match(request.Headers["Range"], @"bytes=(\d*)-(\d*)");
                startIndex = Parse<long>(match.Groups[1].Value);
                responseLength = (Parse<long?>(match.Groups[2].Value) + 1 ?? fileInfo.Length) - startIndex;
                response.StatusCode = (int)HttpStatusCode.PartialContent;
                response.Headers["Content-Range"] = "bytes " + startIndex + "-" + (startIndex + responseLength - 1) + "/" + fileInfo.Length;
            }

            response.AddHeader("content-disposition", "attachment; filename=\"" + Path.GetFileName(filename).Replace(",", "")+"\"");
            response.ContentType = MimeMapping.GetMimeMapping(filename);
            response.Headers["Accept-Ranges"] = "bytes";
            response.Headers["Content-Length"] = responseLength.ToString();
            response.Cache.SetCacheability(HttpCacheability.Public); //required for etag output
            response.Cache.SetETag(etag); //required for IE9 resumable downloads
            response.TransmitFile(filename, startIndex, responseLength);
            if(response.IsClientConnected && startIndex == 0)
                return true;
            return false;
        }

        protected virtual T Parse<T>(object value)
        {
            //convert value to string to allow conversion from types like float to int
            //converter.IsValid only works since .NET4 but still returns invalid values for a few cases like NULL for Unit and not respecting locale for date validation
            try { return (T)System.ComponentModel.TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value.ToString()); }
            catch (Exception) { return default(T); }
        }

        private bool IsCrawlByBot(HttpRequest request)
        {
            var crawlers = new List<string>()
            {
                "googlebot","bingbot","yandexbot","ahrefsbot","msnbot","linkedinbot","exabot","compspybot",
                "yesupbot","paperlibot","tweetmemebot","semrushbot","gigabot","voilabot","adsbot-google",
                "botlink","alkalinebot","araybot","undrip bot","borg-bot","boxseabot","yodaobot","admedia bot",
                "ezooms.bot","confuzzledbot","coolbot","internet cruiser robot","yolinkbot","diibot","musobot",
                "dragonbot","elfinbot","wikiobot","twitterbot","contextad bot","hambot","iajabot","news bot",
                "irobot","socialradarbot","ko_yappo_robot","skimbot","psbot","rixbot","seznambot","careerbot",
                "simbot","solbot","mail.ru_bot","spiderbot","blekkobot","bitlybot","techbot","void-bot",
                "vwbot_k","diffbot","friendfeedbot","archive.org_bot","woriobot","crystalsemanticsbot","wepbot",
                "spbot","tweetedtimes bot","mj12bot","who.is bot","psbot","robot","jbot","bbot","bot"
            };

            if (request.UserAgent != null)
            {
                string ua = request.UserAgent.ToLower();
                bool isCrawler = crawlers.Exists(x => ua.Contains(x));
                return isCrawler;
            }

            return false;
        }

        public override bool IsReusable => false;
    }
}