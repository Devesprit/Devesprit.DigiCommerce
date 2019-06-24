using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.WebFramework.Attributes;

namespace Plugin.Widgets.GoogleAnalytics.Models
{
    public partial class GoogleAnalyticsSettingsModel : ISettings
    {
        [RequiredLocalized()]
        [AllowHtml]
        [DisplayNameLocalized("Plugin.Widgets.GoogleAnalytics.GoogleId")]
        public LocalizedString GoogleId { get; set; } = new LocalizedString("UA-0000000-0");

        [RequiredLocalized()]
        [AllowHtml]
        [DisplayNameLocalized("Plugin.Widgets.GoogleAnalytics.TrackingScript")]
        public LocalizedString TrackingScript { get; set; } = new LocalizedString(@"
<!-- Global site tag (gtag.js) - Google Analytics -->
<script async src=""https://www.googletagmanager.com/gtag/js?id={GOOGLEID}""></script>
<script>
  window.dataLayer = window.dataLayer || [];
  function gtag(){dataLayer.push(arguments);}
  gtag('js', new Date());

  gtag('config', '{GOOGLEID}');
</script>

");

        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.Widgets.GoogleAnalytics.WidgetZone")]
        public string WidgetZone { get; set; } = "head_tag";

        public List<SelectListItem> ZonesList
        {
            get
            {
                var result = new List<SelectListItem>
                {
                    new SelectListItem() {Value = "head_tag", Text = "Head tag"},
                    new SelectListItem() {Value = "body_tag_end", Text = "Before the end of the Body tag"},
                    new SelectListItem() {Value = "body_tag_start", Text = "The beginning of the Body tag"}
                };

                if (!string.IsNullOrWhiteSpace(WidgetZone))
                {
                    var exist = false;
                    foreach (var item in result)
                    {
                        if (item.Value.ToLower().Trim() == WidgetZone.ToLower().Trim())
                        {
                            exist = true;
                            break;
                        }
                    }

                    if (!exist)
                    {
                        result.Add(new SelectListItem() { Value = WidgetZone, Text = WidgetZone });
                    }
                }

                return result;
            }
        }

    }
}
