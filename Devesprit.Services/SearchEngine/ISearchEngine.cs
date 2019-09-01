using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;

namespace Devesprit.Services.SearchEngine
{
    public partial interface ISearchEngine
    {
        Task<SearchResult> SearchAsync(string term, int? filterByCategory = null, int languageId = -1,
            PostType? postType = null,
            SearchPlace searchPlace = SearchPlace.Anywhere, SearchResultSortType orderBy = SearchResultSortType.Score,
            int maxResult = 1000, bool exactSearch = false);

        SearchResult MoreLikeThis(int postId, int? filterByCategory = null, int languageId = -1, PostType? postType = null,
            SearchPlace searchPlace = SearchPlace.Title | SearchPlace.Description,
            int maxResult = 5, SearchResultSortType orderBy = SearchResultSortType.Score);

        Task<SearchResult> AutoCompleteAsync(string prefix, int languageId = -1, int maxResult = 10,
            SearchResultSortType orderBy = SearchResultSortType.LastUpDate);

        long CreateIndex();
    }
}
