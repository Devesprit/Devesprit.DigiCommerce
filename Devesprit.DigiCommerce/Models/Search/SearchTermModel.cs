using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Data.Enums;
using Devesprit.Services.Languages;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Models.Search
{
    public partial class SearchTermModel
    {

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("SearchQuery")]
        public string Query { get; set; }

        [DisplayNameLocalized("Language")]
        public int? LanguageId { get; set; }
        public static IEnumerable<SelectListItem> LanguagesList => DependencyResolver.Current.GetService<ILanguagesService>().GetAsSelectList();

        [DisplayNameLocalized("FilterByCategory")]
        public int? FilterByCategory { get; set; }
        public static IEnumerable<SelectListItem> CategoriesList => DependencyResolver.Current.GetService<IPostCategoriesService>().GetAsSelectList(p=> true);

        [DisplayNameLocalized("SearchIn")]
        public SearchPlace SearchPlace { get; set; } = SearchPlace.Anywhere;

        [DisplayNameLocalized("OrderBy")]
        public SearchResultSortType OrderBy { get; set; } = SearchResultSortType.LastUpDate;

        public int? Page { get; set; } = 1;

        [DisplayNameLocalized("PageSize")]
        public int? PageSize { get; set; } = 20;

        public PostType? PostType { get; set; } = null;

        public static IEnumerable<SelectListItem> PageSizesList => new List<SelectListItem>()
        {
            new SelectListItem() {Value = "10", Text = "10"},
            new SelectListItem() {Value = "20", Text = "20"},
            new SelectListItem() {Value = "30", Text = "30"},
            new SelectListItem() {Value = "40", Text = "40"},
            new SelectListItem() {Value = "50", Text = "50"},
            new SelectListItem() {Value = "75", Text = "75"},
            new SelectListItem() {Value = "100", Text = "100"},
        };
    }
}