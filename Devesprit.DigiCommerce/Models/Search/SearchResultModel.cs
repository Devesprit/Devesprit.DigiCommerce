using System.Collections.Generic;
using Devesprit.DigiCommerce.Models.Post;
using X.PagedList;

namespace Devesprit.DigiCommerce.Models.Search
{
    public partial class SearchResultModel
    {
        public SearchTermModel SearchTerm { get; set; }
        public int NumberOfItemsFound { get; set; }
        public long TimeElapsed { get; set; }
        public IPagedList<PostCardViewModel> SearchResult { get; set; }
        public ViewStyles CardViewStyles { get; set; }
        public List<string> SuggestSimilar { get; set; }
        public bool ShowAdvancedSearchPanel { get; set; } = true;
    }
}