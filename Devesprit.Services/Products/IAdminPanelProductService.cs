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
    public partial interface IAdminPanelProductService: IPostService<TblProducts>
    {
        [MethodCache(Tags = new []{ nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetNewItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) })]
        List<SiteMapEntity> GetNewItemsForSiteMap();

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetPopularItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetHotList(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang")]
        IPagedList<TblProducts> GetFeaturedItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        Task<TblProducts> FindByIdAsync(int id);

        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        Task<TblProducts> FindBySlugAsync(string slug);
        
        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByParam = null, VaryByCustom = "lang", DoNotCacheForAdminUser = true)]
        void GetStatics(out int numberOfPosts, out int numberOfVisits, out DateTime lastUpdate);

        Task IncreaseNumberOfPurchasesAsync(TblProducts product, int value = 1);
    }
}
