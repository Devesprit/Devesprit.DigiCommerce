using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.Services.LicenseManager;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ProductCheckoutAttributeModel
    {
        public int? Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("AttributeName")]
        [AllowHtml]
        public LocalizedString Name { get; set; }

        [DisplayNameLocalized("Description")]
        [AllowHtml]
        public LocalizedString Description { get; set; }

        [DisplayNameLocalized("Required")]
        public bool Required { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("AttributeType")]
        public ProductCheckoutAttributeType AttributeType { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("DisplayOrder")]
        public int DisplayOrder { get; set; }

        [DisplayNameLocalized("UnitPrice")]
        public double UnitPrice { get; set; }
        [DisplayNameLocalized("Minimum")]
        public int? MinRange { get; set; }
        [DisplayNameLocalized("Maximum")]
        public int? MaxRange { get; set; }

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
    }
}