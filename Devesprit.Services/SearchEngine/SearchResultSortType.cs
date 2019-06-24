using System.ComponentModel;

namespace Devesprit.Services.SearchEngine
{
    public enum SearchResultSortType
    {
        [Description("BestMatch")]
        Score,
        [Description("NumberOfVisits")]
        NumberOfVisits,
        [Description("PublishDate")]
        PublishDate,
        [Description("UpdateDate")]
        LastUpDate
    }
}