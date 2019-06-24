using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PageModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("PageHtml")]
        [AllowHtml]
        public LocalizedString Html { get; set; } = new LocalizedString();

        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("PageTitle")]
        public LocalizedString Title { get; set; } = new LocalizedString();

        [DisplayNameLocalized("MetaDescription")]
        public LocalizedString MetaDescription { get; set; } = new LocalizedString();

        [DisplayNameLocalized("MetaKeyword")]
        public LocalizedString MetaKeyword { get; set; } = new LocalizedString();

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("PanelTitle")]
        public LocalizedString PanelTitle { get; set; } = new LocalizedString();

        [RequiredLocalized()]
        [MaxLengthLocalized(150)]
        [DisplayNameLocalized("Slug")]
        public string Slug { get; set; }

        [DisplayNameLocalized("ShowInPanel")]
        public bool ShowInPanel { get; set; } = true;

        [DisplayNameLocalized("ShowInFooter")]
        public bool ShowInFooter { get; set; }

        [DisplayNameLocalized("ShowPageInUserMenuBar")]
        public bool ShowInUserMenuBar { get; set; }

        [DisplayNameLocalized("Published")]
        public bool Published { get; set; } = true;

        [DisplayNameLocalized("ShowAsWebsiteDefaultPage")]
        public bool ShowAsWebsiteDefaultPage { get; set; }
    }
}