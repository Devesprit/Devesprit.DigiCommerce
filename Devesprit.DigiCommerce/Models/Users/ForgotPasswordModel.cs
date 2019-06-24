using Devesprit.Data.Domain;
using Devesprit.Services;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Models.Users
{
    public partial class ForgotPasswordModel
    {
        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [EmailAddressLocalized()]
        [DisplayNameLocalized("EMail")]
        public string Email { get; set; }

        public SiteSettings Settings { get; set; }
        public TblLanguages CurrentLanguage { get; set; }
    }
}