using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PostCategoryModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("CategoryName")]
        public LocalizedString CategoryName { get; set; } = new LocalizedString();

        [DisplayNameLocalized("ParentCategory")]
        public int? ParentCategoryId { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(150)]
        [DisplayNameLocalized("Slug")]
        public string Slug { get; set; }

        [DisplayNameLocalized("ShowInFooter")]
        public bool ShowInFooter { get; set; } = true;

        public List<SelectListItem> CategoriesList { get; set; }
    }
}