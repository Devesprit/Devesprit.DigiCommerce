using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Posts;
using Devesprit.Services.SEO;
using X.PagedList;

namespace Devesprit.Services.Blog
{
    [Intercept(typeof(MethodCache))]
    public partial interface IBlogPostService : IPostService<TblBlogPosts>
    {
        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        IPagedList<TblBlogPosts> GetNewItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) })]
        List<SiteMapEntity> GetNewItemsForSiteMap();

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        IPagedList<TblBlogPosts> GetPopularItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        IPagedList<TblBlogPosts> GetHotList(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        IPagedList<TblBlogPosts> GetFeaturedItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        Task<TblBlogPosts> FindByIdAsync(int id);

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        Task<TblBlogPosts> FindBySlugAsync(string slug);

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByParam = null, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        void GetStatics(out int numberOfPosts, out int numberOfVisits, out DateTime lastUpdate);
    }
}
