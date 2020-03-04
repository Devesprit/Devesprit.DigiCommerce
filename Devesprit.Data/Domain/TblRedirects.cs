using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Redirects")]
    public partial class TblRedirects : BaseEntity
    {
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(450)]
        [Index(IsClustered = false, IsUnique = false)]
        public string RequestedUrl { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(450)]
        [Index(IsClustered = false, IsUnique = false)]
        public string ResponseUrl { get; set; }

        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public MatchType MatchType { get; set; }

        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public ResponseType ResponseType { get; set; }

        public int? RedirectStatus { get; set; }

        public bool IgnoreCase { get; set; }

        public bool AppendQueryString { get; set; }

        public bool AppendLanguageCodeToUrl { get; set; }

        public bool StopProcessingOfSubsequentRules { get; set; }

        public bool Active { get; set; }

        public int Order { get; set; }

        public RedirectRuleGroup RedirectGroup { get; set; }

        public int? EntityId { get; set; }
    }
}
