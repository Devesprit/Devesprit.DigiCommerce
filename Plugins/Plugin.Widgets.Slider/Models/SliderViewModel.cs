using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;

namespace Plugin.Widgets.Slider.Models
{
    public partial class SliderViewModel
    {
        public int? Id { get; set; }

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("ImageUrl")]
        public LocalizedString ImageUrl { get; set; }

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("Plugin.Widgets.Slider.DestinationURL")]
        public LocalizedString Link { get; set; }

        [MaxLengthLocalized(50)]
        [DisplayNameLocalized("Plugin.Widgets.Slider.LinkTarget")]
        public string Target { get; set; } = "_blank";

        [AllowHtml]
        [DisplayNameLocalized("Plugin.Widgets.Slider.OnClickJS")]
        public LocalizedString OnClickJs { get; set; }

        [AllowHtml]
        [DisplayNameLocalized("Title")]
        public LocalizedString Title { get; set; }

        [AllowHtml]
        [DisplayNameLocalized("Description")]
        public LocalizedString Description { get; set; }

        [DisplayNameLocalized("Published")]
        public bool Visible { get; set; } = true;

        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.Widgets.Slider.DisplayZone")]
        public string Zone { get; set; }

        [DisplayNameLocalized("DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        public List<SelectListItem> ZonesList { get
        {
            var result = new List<SelectListItem>
            {
                new SelectListItem() {Value = "layout_start", Text = @"Layout Start (After navigation menu)"},
                new SelectListItem() {Value = "layout_end", Text = @"Layout End (Before footer)"},
                new SelectListItem() {Value = "home_page_start", Text = @"Home page start"},
                new SelectListItem() {Value = "home_page_end", Text = @"Home page end"},
                new SelectListItem() {Value = "blog_home_page_start", Text = @"Blog home page start"},
                new SelectListItem() {Value = "blog_home_page_end", Text = @"Blog home page end"},
                new SelectListItem() {Value = "blog_post_page_start", Text = @"Blog post page start"},
                new SelectListItem() {Value = "blog_post_page_end", Text = @"Blog post page end"},
                new SelectListItem() {Value = "product_page_start", Text = @"Product page start"},
                new SelectListItem() {Value = "product_page_end", Text = @"Product page end"},
            };
            if (!string.IsNullOrWhiteSpace(Zone) )
            {
                var exist = false;
                foreach (var item in result)
                {
                    if (item.Value.ToLower().Trim() == Zone.ToLower().Trim())
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                {
                    result.Add(new SelectListItem() { Value = Zone, Text = Zone });
                }
            }
            return result;
        } }

    }
}
