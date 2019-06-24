using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;

namespace Devesprit.Services.SearchEngine
{
    public partial interface ISearchEngine
    {
        Task<SearchResult> SearchAsync(string term, int? filterByCategory = null, int languageId = 0, PostType? postType = null,
            SearchPlace searchPlace = SearchPlace.Anywhere, int maxResult = 1000);

        SearchResult MoreLikeThis(int postId, int? filterByCategory = null, int languageId = 0, PostType? postType = null,
            SearchPlace searchPlace = SearchPlace.Title | SearchPlace.Description,
            int maxResult = 5);

        Task<SearchResult> AutoCompleteAsync(string prefix, int languageId = 0, int maxResult = 10);

        long CreateIndex();
    }
}
