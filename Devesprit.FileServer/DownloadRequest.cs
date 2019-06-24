using System;

namespace Devesprit.FileServer
{
    public partial class DownloadRequest
    {
        public string File { get; set; }
        public DateTime Expire { get; set; }
        public int DownloadCount { get; set; }
    }
}