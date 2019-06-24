using Devesprit.Data.Enums;
using Devesprit.Data.Events;
using Devesprit.Services.Products;

namespace Devesprit.Services.SearchEngine
{
    public partial class SearchEvent : IEvent
    {
        public string Term { get; }
        public int? FilterByCategory { get; }
        public int LanguageId { get; }
        public PostType? PostType { get; }
        public SearchPlace SearchPlace { get; }
        public int MaxResult { get; }
        public SearchResult Result { get; }

        public SearchEvent(string term, 
            int? filterByCategory, 
            int languageId, 
            PostType? postType,
            SearchPlace searchPlace, 
            int maxResult, 
            SearchResult result)
        {
            Term = term;
            FilterByCategory = filterByCategory;
            LanguageId = languageId;
            PostType = postType;
            SearchPlace = searchPlace;
            MaxResult = maxResult;
            Result = result;
        }
    }
}
