using System.ComponentModel;

namespace Devesprit.DigiCommerce.Models.Post
{
    public enum PostsListType
    {
        [Description("NewestPosts")]
        Newest,
        [Description("MostPopularPosts")]
        MostPopular,
        [Description("HotPosts")]
        HotList,
        [Description("FeaturedPosts")]
        Featured
    }
}