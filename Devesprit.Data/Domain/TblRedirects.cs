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
        public string RequestedUrl { get; set; }

        [Required]
        public string ResponseUrl { get; set; }

        [Required]
        public MatchType MatchType { get; set; }

        [Required]
        public ResponseType ResponseType { get; set; }

        public RedirectStatusCode? RedirectStatus { get; set; }

        public bool IgnoreCase { get; set; }

        public bool AppendQueryString { get; set; }

        public bool StopProcessingOfSubsequentRules { get; set; }

        public bool Active { get; set; }

        public int Order { get; set; }

        public RedirectRuleGroup RedirectGroup { get; set; }

        public int? EntityId { get; set; }
    }
}
