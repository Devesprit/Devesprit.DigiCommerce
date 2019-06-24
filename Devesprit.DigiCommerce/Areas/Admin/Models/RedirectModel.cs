using Devesprit.Data.Enums;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public class RedirectModel
    {
        public int? Id { get; set; }

        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("RuleName")]
        public string Name { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("RequestedUrl")]
        public string RequestedUrl { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("ResponseUrl")]
        public string ResponseUrl { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("MatchType")]
        public MatchType MatchType { get; set; } = MatchType.Regex;

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("ResponseType")]
        public ResponseType ResponseType { get; set; } = ResponseType.Rewrite;

        [DisplayNameLocalized("RedirectStatus")]
        public RedirectStatusCode? RedirectStatus { get; set; }

        [DisplayNameLocalized("IgnoreCase")]
        public bool IgnoreCase { get; set; } = true;

        [DisplayNameLocalized("AppendQueryString")]
        public bool AppendQueryString { get; set; }

        [DisplayNameLocalized("StopProcessingOfSubsequentRules")]
        public bool StopProcessingOfSubsequentRules { get; set; }

        [DisplayNameLocalized("Active")]
        public bool Active { get; set; } = true;

        [DisplayNameLocalized("Order")]
        public int Order { get; set; }

        public RedirectRuleGroup RedirectGroup { get; set; } = RedirectRuleGroup.None;

        public int? EntityId { get; set; }
    }
}