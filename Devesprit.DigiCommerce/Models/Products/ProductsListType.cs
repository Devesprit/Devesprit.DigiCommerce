using System.ComponentModel;

namespace Devesprit.DigiCommerce.Models.Products
{
    public enum ProductsListType
    {
        [Description("NewestProducts")]
        Newest,
        [Description("MostPopularProducts")]
        MostPopular,
        [Description("HotProducts")]
        HotList,
        [Description("FeaturedProducts")]
        Featured,
        [Description("BestSellingProducts")]
        BestSelling
    }
}