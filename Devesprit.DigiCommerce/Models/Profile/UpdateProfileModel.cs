using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Models.Profile
{
    public partial class UpdateProfileModel
    {
        public string Id { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("FirstName")]
        public string FirstName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("LastName")]
        public string LastName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [EmailAddressLocalized()]
        [DisplayNameLocalized("EMail")]
        public string Email { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Country")]
        public int UserCountryId { get; set; }

        public List<SelectListItem> CountriesList { get; set; }

        public bool MustConfirmNewEmail { get; set; }

        [DisplayNameLocalized("Avatar")]
        [FileExtensions("png,jpg,jpeg", "SelectImageFile")]
        [MaxFileSize(500 * 1024)]
        public HttpPostedFileBase Avatar { get; set; }
    }
}