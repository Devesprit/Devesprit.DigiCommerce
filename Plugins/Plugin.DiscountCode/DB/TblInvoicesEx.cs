using System;
using System.ComponentModel.DataAnnotations;
using Devesprit.Data;
using Devesprit.Data.Domain;

namespace Plugin.DiscountCode.DB
{
    public class TblInvoicesDiscountCode : BaseEntity
    {
        [Required]
        public Guid InvoiceId { get; set; }

        public virtual TblInvoices Invoice { get; set; }        
        
        [Required, MaxLength(250)]
        public string DiscountCode { get; set; }
    }
}