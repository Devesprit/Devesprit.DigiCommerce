using System.Collections.Generic;

namespace Devesprit.Services.AdminAreaMenu
{
    public partial class AdminMenuItem
    {
        public string MenuDisplayName { get; set; }
        public string DestUrl { get; set; }
        public string OnClickJs { get; set; }
        public string Icon { get; set; }
        public string Tooltip { get; set; }
        public string Target { get; set; }
        public int DisplayOrder { get; set; }
        public string NeedPermission { get; set; }
        public List<AdminMenuItem> SubMenus { get; set; }
    }
}