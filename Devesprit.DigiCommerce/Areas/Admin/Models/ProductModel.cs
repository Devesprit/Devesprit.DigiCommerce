using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.FileServers;
using Devesprit.Services.LicenseManager;
using Devesprit.Services.Posts;
using Devesprit.Services.Users;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ProductModel
    {
        public int? Id { get; set; }


        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("ProductTitle")]
        public LocalizedString Title { get; set; }


        [DisplayNameLocalized("Tags")]
        public string[] ProductTags { get; set; } = new string[]{};


        [DisplayNameLocalized("Categories")]
        public int[] ProductCategories { get; set; } = new int[]{};


        [DisplayNameLocalized("PublishDate")]
        public DateTime PublishDate { get; set; } = DateTime.Now;


        [DisplayNameLocalized("Update")]
        public DateTime? LastUpDate { get; set; } = DateTime.Now;


        [DisplayNameLocalized("Published")]
        public bool Published { get; set; } = true;


        [DisplayNameLocalized("Featured")]
        public bool IsFeatured { get; set; }


        [DisplayNameLocalized("ShowSimilarCases")]
        public bool ShowSimilarCases { get; set; } = true;


        [DisplayNameLocalized("ShowKeywords")]
        public bool ShowKeywords { get; set; } = true;


        [DisplayNameLocalized("HotList")]
        public bool ShowInHotList { get; set; }


        [DisplayNameLocalized("Pinned")]
        public bool PinToTop { get; set; }


        [DisplayNameLocalized("AllowCustomerReviews")]
        public bool AllowCustomerReviews { get; set; } = true;
        

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Visits")]
        public int NumberOfViews { get; set; }


        [DisplayNameLocalized("FilesPath")]
        public string FilesPath { get; set; }


        [DisplayNameLocalized("DemoFilesPath")]
        public string DemoFilesPath { get; set; }

        [DisplayNameLocalized("UserMustLoggedInToDownloadFiles")]
        public bool UserMustLoggedInToDownloadFiles { get; set; }

        [DisplayNameLocalized("AlwaysShowDownloadButton")]
        public bool AlwaysShowDownloadButton { get; set; } = true;

        [DisplayNameLocalized("UserMustLoggedInToDownloadDemoFiles")]
        public bool UserMustLoggedInToDownloadDemoFiles { get; set; }


        [DisplayNameLocalized("FileServer")]
        public int? FileServerId { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("BasePrice")]
        public double Price { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("RenewalBasePrice")]
        public double RenewalPrice { get; set; }


        [DisplayNameLocalized("PurchaseExpiration")]
        public int? PurchaseExpiration { get; set; }

        public TimePeriodType? PurchaseExpirationTimeType { get; set; }


        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("PageTitle")]
        public LocalizedString PageTitle { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Slug")]
        public string Slug { get; set; }


        [DisplayNameLocalized("AlternativeSlugs")]
        public string AlternativeSlugsStr { get; set; }


        [DisplayNameLocalized("MetaDescription")]
        public LocalizedString MetaDescription { get; set; }


        [DisplayNameLocalized("MetaKeyword")]
        public LocalizedString MetaKeyWords { get; set; }


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
                var fileServersService = DependencyResolver.Current.GetService<IFileServersService>();
                return fileServersService.GetAsSelectList();
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

        public List<SelectListItem> ProductCategoriesList => DependencyResolver.Current
            .GetService<IPostCategoriesService>().GetAsSelectList(p =>
                p.DisplayArea == DisplayArea.ProductsSection || p.DisplayArea == DisplayArea.Both);
    }
}