using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Products")]
    public partial class TblProducts: TblPosts
    {
        public TblProducts()
        {
            PostType = Enums.PostType.Product;
        }
        [Index(IsClustered = false, IsUnique = false)]
        public double Price { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public int NumberOfDownloads { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public int NumberOfPurchases { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public double RenewalPrice { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public int? PurchaseExpiration { get; set; }
        public TimePeriodType? PurchaseExpirationTimeType { get; set; }
        public string FilesPath { get; set; }
        public string DemoFilesPath { get; set; }
        public bool UserMustLoggedInToDownloadFiles { get; set; }
        public bool AlwaysShowDownloadButton { get; set; }
        public bool UserMustLoggedInToDownloadDemoFiles { get; set; }
        public int? FileServerId { get; set; }
        public virtual TblFileServers FileServer { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public int? DownloadLimitedToUserGroupId { get; set; }
        public virtual TblUserGroups DownloadLimitedToUserGroup { get; set; }
        public bool HigherUserGroupsCanDownload { get; set; }
        public string LicenseGeneratorServiceId { get; set; }
        public string FilesListJson { get; set; }


        public virtual ICollection<TblProductDownloadsLog> DownloadsLog { get; set; }
        public virtual ICollection<TblProductCheckoutAttributes> CheckoutAttributes { get; set; }
        public virtual ICollection<TblProductDiscountsForUserGroups> DiscountsForUserGroups { get; set; }
    }
}
