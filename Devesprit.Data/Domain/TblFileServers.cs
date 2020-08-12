using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_FileServers")]
    public partial class TblFileServers : BaseEntity
    {
        [Required, MaxLength(500)]
        public string FileServerName { get; set; }
        [Required, MaxLength(500)]
        public string FileServerUrl { get; set; }
        [Required, MaxLength(500)]
        public string FileUploadServerUrl { get; set; }
        [Required, MaxLength(500)]
        public string ServiceUserName { get; set; }
        [Required, MaxLength(500)]
        public string ServicePassword { get; set; }
        [Required, MaxLength(500)]
        public string EncryptionKey { get; set; }
        [Required, MaxLength(500)]
        public string EncryptionSalt { get; set; }

        public string DownloadPageUrl
        {
            get
            {
                if (Uri.TryCreate(FileServerUrl, UriKind.Absolute, out Uri uri))
                {
                    return (uri.Scheme + Uri.SchemeDelimiter + uri.Host +
                            (uri.Port > 0 && uri.Port != 443 && uri.Port != 80 ? ":" + uri.Port : "")).TrimEnd('/') +
                           "/Download.ashx";
                }

                return string.Empty;
            }
        }
    }
}
