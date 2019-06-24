using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.Data.Enums;
using Devesprit.Services.Countries;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Models.Invoice
{
    public partial class InvoiceBillingAddressModel
    {
        public InvoiceStatus InvoiceStatus { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("FirstName")]
        public string FirstName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("LastName")]
        public string LastName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("CompanyName")]
        public string CompanyName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("PhoneNumber")]
        public string PhoneNumber { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("ZipCode")]
        public string ZipCode { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("StateProvince")]
        public string State { get; set; }

        
        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("City")]
        public string City { get; set; }

        
        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(1000)]
        [DisplayNameLocalized("StreetAddress")]
        public string StreetAddress { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [EmailAddressLocalized()]
        [DisplayNameLocalized("EMail")]
        public string Email { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("Country")]
        public int CountryId { get; set; }

        public List<SelectListItem> CountriesList
        {
            get
            {
                var countriesService = DependencyResolver.Current.GetService<ICountriesService>();
                return countriesService.GetAsSelectList();
            }
        }

        public bool IsEmpty => FirstName.IsNullOrWhiteSpace() &&
                               LastName.IsNullOrWhiteSpace() &&
                               CompanyName.IsNullOrWhiteSpace() &&
                               Email.IsNullOrWhiteSpace() &&
                               PhoneNumber.IsNullOrWhiteSpace() &&
                               StreetAddress.IsNullOrWhiteSpace();
    }
}