using System;
using System.Collections.Generic;

namespace Devesprit.Services.SearchEngine
{
    public partial class SearchResult
    {
        public long ElapsedMilliseconds { get; set; }
        public Exception Error { get; set; }
        public bool HasError { get; set; }
        public List<SearchResultDocument> Documents { get; set; } = new List<SearchResultDocument>();
        public List<string> SuggestSimilar { get; set; } = new List<string>();
    }
}