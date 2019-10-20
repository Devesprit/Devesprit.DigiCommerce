using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Services.Posts;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class BlogPostModel
    {
        public int? Id { get; set; }


        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("Title")]
        public LocalizedString Title { get; set; }


        [DisplayNameLocalized("Tags")]
        public string[] PostTags { get; set; } = new string[]{};


        [DisplayNameLocalized("Categories")]
        public int[] PostCategories { get; set; } = new int[] { };


        [DisplayNameLocalized("PublishDate")]
        public DateTime PublishDate { get; set; } = DateTime.Now;


        [DisplayNameLocalized("Update")]
        public DateTime? LastUpDate { get; set; } = DateTime.Now;


        [DisplayNameLocalized("Published")]
        public bool Published { get; set; } = true;


        [DisplayNameLocalized("Featured")]
        public bool IsFeatured { get; set; }


        [DisplayNameLocalized("ShowSimilarCases")]
        public bool ShowSimilarCases { get; set; } = true;


        [DisplayNameLocalized("ShowKeywords")]
        public bool ShowKeywords { get; set; } = true;


        [DisplayNameLocalized("HotList")]
        public bool ShowInHotList { get; set; }


        [DisplayNameLocalized("Pinned")]
        public bool PinToTop { get; set; }


        [DisplayNameLocalized("AllowCustomerReviews")]
        public bool AllowCustomerReviews { get; set; } = true;
        

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Visits")]
        public int NumberOfViews { get; set; }
        

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("PageTitle")]
        public LocalizedString PageTitle { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Slug")]
        public string Slug { get; set; }


        [DisplayNameLocalized("MetaDescription")]
        public LocalizedString MetaDescription { get; set; }


        [DisplayNameLocalized("MetaKeyword")]
        public LocalizedString MetaKeyWords { get; set; }

        public List<SelectListItem> PostCategoriesList => DependencyResolver.Current.GetService<IPostCategoriesService>().GetAsSelectList();
    }
}