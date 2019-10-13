using System;
using System.Collections.Generic;
using Devesprit.Data.Domain;
using Devesprit.Services.Products;

namespace Devesprit.DigiCommerce.Models.Products
{
    public partial class ProductDownloadModel
    {
        public int ProductId { get; set; }
        public bool HasDownloadableFile { get; set; }
        public double PriceForCurrentUser { get; set; }
        public TblUserGroups CurrentUserGroup { get; set; }
        public bool HasDemoVersion { get; set; }
        public TblUserGroups DownloadLimitedToUserGroup { get; set; }
        public bool HigherUserGroupsCanDownload { get; set; }
        public List<Tuple<TblUserGroups, string>> DiscountForUserGroupsDescription { get; set; }
        public bool CanDownloadByCurrentUser { get; set; }
        public ProductService.UserCanDownloadProductResult DownloadBlockingReason { get; set; }
        public bool AlwaysShowDownloadButton { get; set; }
        public bool ShowUpgradeUserAccountBtn { get; set; }
        public bool ShowDownloadFullVersionBtn { get; set; }
        public bool ShowPurchaseBtn { get; set; }
        public bool ShowDownloadDemoVersionBtn { get; set; }
        public bool CurrentUserHasAlreadyPurchasedThisProduct { get; set; }
    }
}