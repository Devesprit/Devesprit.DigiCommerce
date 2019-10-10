using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.SEO;
using X.PagedList;

namespace Devesprit.Services.Posts
{
    public partial interface IPostService<T> where T : TblPosts
    {
        IPagedList<T> GetItemsById(List<int> ids, int pageIndex = 1, int pageSize = int.MaxValue);
        IPagedList<T> GetNewItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);
        List<SiteMapEntity> GetNewItemsForSiteMap();
        IPagedList<T> GetPopularItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);
        IPagedList<T> GetHotList(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);
        IPagedList<T> GetFeaturedItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null);

        Task<T> FindByIdAsync(int id);
        Task<T> FindBySlugAsync(string slug);
        IQueryable<T> GetAsQueryable();
        Task DeleteAsync(int id);
        Task UpdateAsync(T record);
        Task<int> AddAsync(T record);
        Task IncreaseNumberOfViewsAsync(T post, int value = 1);
        Task UpdatePostTagsAsync(int postId, List<string> tagsList);
        Task UpdatePostCategoriesAsync(int postId, List<int> categoriesList);
        void GetStatics(out int numberOfPosts, out int numberOfVisits, out DateTime lastUpdate);
    }
}
