using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Models.ContactUs
{
    public partial class ContactUsModel
    {
        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("YourName")]
        public string Name { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("YourEmail")]
        [EmailAddressLocalized]
        public string Email { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("Subject")]
        public string Subject { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(2000)]
        [DisplayNameLocalized("Message")]
        public string Message { get; set; }
    }
}