using Devesprit.Data.Domain;

namespace Devesprit.DigiCommerce.Models.Products
{
    public partial class ProductCardDownloadModel
    {
        public int ProductId { get; set; }
        public double PriceForCurrentUser { get; set; }
        public TblUserGroups DownloadLimitedToUserGroup { get; set; }
        public bool ShowPurchaseBtn { get; set; }
        public bool HigherUserGroupsCanDownload { get; set; }
    }
}