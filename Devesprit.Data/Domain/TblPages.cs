using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Pages")]
    public partial class TblPages : BaseEntity
    {
        [Required]
        public string Html { get; set; }
        [Required]
        public string Title { get; set; }
        [Required, 
         StringLength(500),
         Index(IsUnique = true),
        Column(TypeName = "NVARCHAR")]
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeyword { get; set; }
        public bool ShowInPanel { get; set; }
        public bool ShowInFooter { get; set; }
        public bool ShowInUserMenuBar { get; set; }
        public string PanelTitle { get; set; }
        public bool Published { get; set; }
        public bool ShowAsWebsiteDefaultPage { get; set; }
    }
}
