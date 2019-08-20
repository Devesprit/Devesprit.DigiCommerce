using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.Services;
using Devesprit.Services.ExternalLoginProvider;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Models.Users
{
    public partial class SignUpModel
    {
        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("FirstName")]
        public string FName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("LastName")]
        public string LName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [EmailAddressLocalized()]
        [DisplayNameLocalized("EMail")]
        public string Email { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Password")]
        [DataType(DataType.Password)]
        [MaxLengthLocalized(100)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [RequiredLocalized(AllowEmptyStrings = false)]
        [CompareLocalized("Password", "PasswordNotMatch")]
        [DisplayNameLocalized("ConfirmPassword")]
        [MaxLengthLocalized(100)]
        public string PasswordConfirm { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Country")]
        public int Country { get; set; }


        [DisplayNameLocalized("AcceptTerms")]
        [MustBeTrueLocalized("MustAcceptTerms")]
        public bool AcceptTerms { get; set; }
        public bool UserMustAcceptTerms { get; set; }
        public TblLanguages CurrentLanguage { get; set; }
        public List<SelectListItem> CountriesList { get; set; }
        public List<ExternalLoginProviderInfo> ExternalLoginProviders { get; set; }
    }
}