using System;

namespace Devesprit.FileServer.Domain
{
    public partial class DownloadLog
    {
        public DateTime Date { get; set; }
        public string RequestQueryString { get; set; }
        public string ClientIp { get; set; }
        public string FilePath { get; set; }
        public long Id { get; set; }
    }
}