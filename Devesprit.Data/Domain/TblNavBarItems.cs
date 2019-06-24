using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_NavBarItems")]
    public partial class TblNavBarItems : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string InnerHtml { get; set; }
        public string Tooltip { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public string OnClickJs { get; set; }
        public string Icon { get; set; }
        public int Index { get; set; }
        public int? ParentItemId { get; set; }
        public virtual TblNavBarItems ParentItem { get; set; }
        public virtual ICollection<TblNavBarItems> SubItems { get; set; }
    }
}
