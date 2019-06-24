using System.ComponentModel.DataAnnotations;
using Devesprit.Data.Domain;
using Devesprit.Services;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Models.Users
{
    public partial class ResetPasswordModel
    {
        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("NewPassword")]
        [DataType(DataType.Password)]
        [MaxLengthLocalized(100)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [RequiredLocalized(AllowEmptyStrings = false)]
        [CompareLocalized("Password", "PasswordNotMatch")]
        [DisplayNameLocalized("ConfirmPassword")]
        [MaxLengthLocalized(100)]
        public string PasswordConfirm { get; set; }

        public string Code { get; set; }
        public string UserId { get; set; }

        public SiteSettings Settings { get; set; }
        public TblLanguages CurrentLanguage { get; set; }
    }
}