using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Devesprit.Data.Enums;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class UserModel
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

        [DisplayNameLocalized("NewPassword")]
        [DataType(DataType.Password)]
        [MaxLengthLocalized(100)]
        public string Password { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [EmailAddressLocalized()]
        [DisplayNameLocalized("EMail")]
        public string Email { get; set; }


        [DisplayNameLocalized("EmailConfirmed")]
        public bool EmailConfirmed { get; set; }


        [DisplayNameLocalized("UserDisabled")]
        public bool UserDisabled { get; set; }


        [DisplayNameLocalized("LockoutEnabled")]
        public bool LockoutEnabled { get; set; }


        [DisplayNameLocalized("IsAdmin")]
        public bool IsAdmin { get; set; }
        [DisplayNameLocalized("UserRole")]
        public int? RoleId { get; set; }


        [DisplayNameLocalized("LockoutEndDate")]
        public DateTime? LockoutEndDateUtc { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Country")]
        public int? UserCountryId { get; set; }


        [DisplayNameLocalized("UserGroup")]
        public int? UserGroupId { get; set; }

        [DisplayNameLocalized("NumberOfDownload")]
        public int? MaxDownloadCount { get; set; }
        [DisplayNameLocalized("Per")]
        public TimePeriodType? MaxDownloadPeriodType { get; set; }


        [DisplayNameLocalized("SubscriptionDate")]
        public DateTime? SubscriptionDate { get; set; }


        [DisplayNameLocalized("ExpiryDate")]
        public DateTime? SubscriptionExpireDate { get; set; }


        [DisplayNameLocalized("RegistrationDate")]
        public DateTime RegisterDate { get; set; }


        [DisplayNameLocalized("Avatar")]
        [WebFramework.Attributes.FileExtensions("png,jpg,jpeg","SelectImageFile")]
        [MaxFileSize(500 * 1024)]
        public HttpPostedFileBase Avatar { get; set; }
        public string CurrentAvatarUrl { get; set; }

        [DisplayNameLocalized("UserDisableReason")]
        public string DisableReason { get; set; }
        public DateTime? UserLastLoginDate { get; set; }
        public string UserLatestIP { get; set; }


        public List<SelectListItem> CountriesList { get; set; }
        public List<SelectListItem> UserGroupsList { get; set; }
        public List<SelectListItem> UserRolesList { get; set; }
    }
    
}