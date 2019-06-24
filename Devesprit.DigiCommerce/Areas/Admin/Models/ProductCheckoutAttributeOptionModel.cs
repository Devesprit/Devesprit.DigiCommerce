using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;

using Devesprit.Services.FileServers;
using Devesprit.Services.LicenseManager;
using Devesprit.Services.Users;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ProductCheckoutAttributeOptionModel
    {
        public int? Id { get; set; }

        [Required]
        public int ProductCheckoutAttributeId { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("OptionName")]
        [AllowHtml]
        public LocalizedString Name { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("BasePrice")]
        public double Price { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("RenewalBasePrice")]
        public double RenewalPrice { get; set; }


        [DisplayNameLocalized("PurchaseExpiration")]
        public int? PurchaseExpiration { get; set; }

        public TimePeriodType? PurchaseExpirationTimeType { get; set; }


        [DisplayNameLocalized("FilesPath")]
        public string FilesPath { get; set; }

        [DisplayNameLocalized("FileServer")]
        public int? FileServerId { get; set; }

        [DisplayNameLocalized("DownloadLimitsByUserGroups")]
        public int? DownloadLimitedToUserGroupId { get; set; }


        [DisplayNameLocalized("HigherUserGroupsCanDownload")]
        public bool HigherUserGroupsCanDownload { get; set; }

        [DisplayNameLocalized("LicenseGeneratorService")]
        public string LicenseGeneratorServiceId { get; set; }


        public List<SelectListItem> LicenseGeneratorsList
        {
            get
            {
                var licenseManager = DependencyResolver.Current.GetService<ILicenseManager>();
                return licenseManager.GetAvailableLicenseGenerators().Select(p => new SelectListItem()
                {
                    Value = p.LicenseGeneratorServiceId,
                    Text = p.LicenseGeneratorServiceId
                }).ToList();
            }
        }

        public List<SelectListItem> FileServersList
        {
            get
            {
                var fileServersServicef = DependencyResolver.Current.GetService<IFileServersService>();
                return fileServersServicef.GetAsSelectList();
            }
        }

        public List<SelectListItem> UserGroupsList
        {
            get
            {
                var userGroupsService = DependencyResolver.Current.GetService<IUserGroupsService>();
                return userGroupsService.GetAsSelectList();
            }
        }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}