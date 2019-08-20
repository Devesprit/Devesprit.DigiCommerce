using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.SEO;
using X.PagedList;

namespace Devesprit.Services.Posts
{
    public partial interface IPostService<T> where T : TblPosts
    {
        IPagedList<T> GetItemsById(List<int> ids, int pageIndex = 1,
            int pageSize = int.MaxValue, SearchResultSortType sortType = SearchResultSortType.Score);
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
        int GetNumberOfLikes(int postId);
        Task UpdatePostTagsAsync(int postId, List<string> tagsList);
        Task UpdatePostCategoriesAsync(int postId, List<int> categoriesList);
        void GetStatics(out int numberOfPosts, out int numberOfVisits, out DateTime lastUpdate);
    }
}
