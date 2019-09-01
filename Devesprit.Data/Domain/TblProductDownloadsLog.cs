using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_ProductDownloadsLog")]
    public partial class TblProductDownloadsLog: BaseEntity
    {
        [Index(IsClustered = false, IsUnique = false)]
        public DateTime DownloadDate { get; set; }
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public int ProductId { get; set; }
        public virtual TblProducts Product { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public string UserId { get; set; }
        public virtual TblUsers User { get; set; }
        public string UserIp { get; set; }
        public string DownloadLink { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public bool IsDemoVersion { get; set; }
    }
}
