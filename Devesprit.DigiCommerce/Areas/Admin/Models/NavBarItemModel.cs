using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class NavBarItemModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("Name")]
        public string Name { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("LinkTitle")]
        [AllowHtml]
        public LocalizedString InnerHtml { get; set; }

        [DisplayNameLocalized("Tooltip")]
        public LocalizedString Tooltip { get; set; }

        [DisplayNameLocalized("Url")]
        public LocalizedString Url { get; set; }

        [DisplayNameLocalized("Target")]
        public string Target { get; set; }

        [DisplayNameLocalized("Icon")]
        [AllowHtml]
        public LocalizedString Icon { get; set; }

        [DisplayNameLocalized("DisplayArea")]
        public DisplayArea DisplayArea { get; set; } = DisplayArea.Both;

        [DisplayNameLocalized("NavBarItemOnClickJs")]
        [AllowHtml]
        public LocalizedString OnClickJs { get; set; }
        public int? ParentItemId { get; set; }
        public int Index { get; set; } = -1;

        public List<SelectListItem> TargetsList => new List<SelectListItem>()
        {
            new SelectListItem(){Text = "_self", Value = "_self"},
            new SelectListItem(){Text = "_blank", Value = "_blank"},
            new SelectListItem(){Text = "_parent", Value = "_parent"},
            new SelectListItem(){Text = "_top", Value = "_top"}
        };
    }
}