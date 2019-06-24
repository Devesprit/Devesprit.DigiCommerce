using System.ComponentModel.DataAnnotations;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Models.Profile
{
    public partial class ChangePasswordModel
    {
        public string Id { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("CurrentPassword")]
        [DataType(DataType.Password)]
        [MaxLengthLocalized(100)]
        public string CurrentPassword { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("NewPassword")]
        [DataType(DataType.Password)]
        [MaxLengthLocalized(100)]
        public string NewPassword { get; set; }


        [DataType(DataType.Password)]
        [RequiredLocalized(AllowEmptyStrings = false)]
        [CompareLocalized("NewPassword", "PasswordNotMatch")]
        [DisplayNameLocalized("ConfirmPassword")]
        [MaxLengthLocalized(100)]
        public string PasswordConfirm { get; set; }

    }
}