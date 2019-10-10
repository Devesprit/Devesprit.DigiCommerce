using System.Threading.Tasks;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Enums;
using Devesprit.Services.MemoryCache;

namespace Devesprit.Services.SearchEngine
{
    [Intercept(typeof(MethodCache))]
    public partial interface ISearchEngine
    {
        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        Task<SearchResult> SearchAsync(string term, int? filterByCategory = null, int languageId = -1,
            PostType? postType = null,
            SearchPlace searchPlace = SearchPlace.Anywhere, SearchResultSortType orderBy = SearchResultSortType.Score,
            int maxResult = 1000, bool exactSearch = false);

        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        SearchResult MoreLikeThis(int postId, int? filterByCategory = null, int languageId = -1, PostType? postType = null,
            SearchPlace searchPlace = SearchPlace.Title | SearchPlace.Description,
            int maxResult = 5, SearchResultSortType orderBy = SearchResultSortType.Score);

        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        Task<SearchResult> AutoCompleteAsync(string prefix, int languageId = -1, int maxResult = 10,
            SearchResultSortType orderBy = SearchResultSortType.LastUpDate);

        long CreateIndex();
    }
}
