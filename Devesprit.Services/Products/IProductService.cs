using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Posts;
using Devesprit.Services.SEO;
using X.PagedList;

namespace Devesprit.Services.Products
{
    [Intercept(typeof(MethodCache))]
    public partial interface IProductService : IPostService<TblProducts>
    {
        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetNewItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) })]
        List<SiteMapEntity> GetNewItemsForSiteMap();

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetPopularItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetHotList(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetFeaturedItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetBestSelling(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetMostDownloadedItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetFreeItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        Task<TblProducts> FindByIdAsync(int id);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        Task<TblProducts> FindBySlugAsync(string slug);

        Task IncreaseNumberOfDownloadsAsync(TblProducts product, int value = 1);

        Task IncreaseNumberOfPurchasesAsync(TblProducts product, int value = 1);

        double CalculateProductPriceForUser(TblProducts product, TblUsers user);
        UserCanDownloadProductResult UserCanDownloadProduct(TblProducts product, TblUsers user, bool demoFiles);
        Task<List<TblProductCheckoutAttributeOptions>> GetUserDownloadableAttributesAsync(TblProducts product,
            TblUsers user);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByParam = null, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        void GetStatics(out int numberOfPosts, out int numberOfVisits, out DateTime lastUpdate);
    }
}