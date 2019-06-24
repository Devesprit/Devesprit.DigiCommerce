using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Devesprit.FileServer.Domain;
using Devesprit.FileServer.Repository.Interfaces;
using Devesprit.Utilities.Extensions;
using LiteDB;

namespace Devesprit.FileServer.Repository
{
    public partial class FileManagerRepository: IFileManagerRepository
    {
        private LiteDatabase _db;
        private LiteDatabase Db => _db ?? (_db = new LiteDatabase(GetConnectionString()));

        private static string GetConnectionString()
        {
            var appDataDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\')+ (@"\App_Data");
            if (!Directory.Exists(appDataDir))
            {
                Directory.CreateDirectory(appDataDir);
            }

            return appDataDir + @"\DownloadsLog.db";
        }

        public virtual async Task LogDownloadRequest(string filePath, string downloadQueryString, HttpContext currentContext)
        {
            await Task.Run(() =>
            {
                var downloadLog = Db.GetCollection<DownloadLog>("DownloadLogs");
                downloadLog.Insert(new DownloadLog()
                {
                    Date = DateTime.Now,
                    FilePath = filePath,
                    ClientIp = currentContext.GetClientIpAddress(),
                    RequestQueryString = downloadQueryString,
                    Id = DateTime.Now.Ticks + new Random().Next()
                });

                downloadLog.EnsureIndex(x => x.RequestQueryString);
            });
        }

        public virtual async Task<int> GetDownloadCount(string request)
        {
            return await Task.Run(() =>
            {
                var downloadLog = Db.GetCollection<DownloadLog>("DownloadLogs");
                return downloadLog.Find(x => x.RequestQueryString == request).Select(x => x.Id).Distinct()
                    .Count();
            });
        }
    }
}