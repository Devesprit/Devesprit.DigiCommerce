namespace Devesprit.Services.SearchEngine
{
    public partial class SearchResultDocument
    {
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentBody { get; set; }
        public string DocumentTags { get; set; }
        public string DocumentKeywords { get; set; }
        public float Score { get; set; }
        public string LanguageIsoCode { get; set; }
        public int LanguageId { get; set; }
    }
}