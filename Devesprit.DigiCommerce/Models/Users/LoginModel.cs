using System.Collections.Generic;
using Devesprit.Data.Domain;
using Devesprit.Services.ExternalLoginProvider;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Models.Users
{
    public partial class LoginModel
    {
        [RequiredLocalized(AllowEmptyStrings = false)] 
        [MaxLengthLocalized(250)]
        [EmailAddressLocalized()]
        [DisplayNameLocalized("EMail")]
        public string Email { get; set; }
        
        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("Password")]
        public string Password { get; set; }

        [DisplayNameLocalized("RememberMe")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
        public TblLanguages CurrentLanguage { get; set; }
        public List<ExternalLoginProviderInfo> ExternalLoginProviders { get; set; }
    }
}