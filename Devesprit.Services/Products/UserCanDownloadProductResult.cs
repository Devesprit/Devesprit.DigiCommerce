using System;

namespace Devesprit.Services.Products
{
    public partial class ProductService
    {
        [Flags]
        public enum UserCanDownloadProductResult
        {
            None = 0,
            UserMustLoggedIn = 1,
            UserMustSubscribeToAPlan = 2,
            UserMustSubscribeToAPlanOrHigher = 4,
            UserMustPurchaseTheProduct = 8,
            UserCanDownloadProduct = 16,
            UserDownloadLimitReached = 32,
            UserGroupDownloadLimitReached = 64,
        }
    }
}