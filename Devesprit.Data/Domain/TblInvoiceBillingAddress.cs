using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_InvoiceBillingAddress")]
    public partial class TblInvoiceBillingAddress: BaseEntity
    {
        [Required]
        public Guid InvoiceId { get; set; }
        public virtual TblInvoices Invoice { get; set; }

        [Required, MaxLength(200)]
        public string FirstName { get; set; }
        [Required, MaxLength(200)]
        public string LastName { get; set; }
        [MaxLength(200)]
        public string CompanyName { get; set; }
        [Required, MaxLength(500)]
        public string Email { get; set; }
        [Required, MaxLength(50)]
        public string PhoneNumber { get; set; }
        [Required, MaxLength(100)]
        public string ZipCode { get; set; }
        [Required, MaxLength(200)]
        public string State { get; set; }
        [Required, MaxLength(200)]
        public string City { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public int CountryId { get; set; }
        public virtual TblCountries Country { get; set; }
    }
}