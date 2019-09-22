using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Invoices")]
    public partial class TblInvoices
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Index(IsClustered = false, IsUnique = false)]
        public string UserId { get; set; }
        public virtual TblUsers User { get; set; }
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public DateTime CreateDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double? DiscountAmount { get; set; }
        public string DiscountDescription { get; set; }
        public double? TotalTaxAmount { get; set; }
        public string TaxDescription { get; set; }
        public double PaidAmount { get; set; }
        public int? CurrencyId { get; set; }
        public virtual TblCurrencies Currency { get; set; }
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public InvoiceStatus Status { get; set; }
        [MaxLength(200)]
        public string PaymentGatewayName { get; set; }
        [MaxLength(200)]
        public string PaymentGatewaySystemName { get; set; }
        public string PaymentGatewayTransactionId { get; set; }
        public string PaymentGatewayToken { get; set; }
        public string InvoiceNote { get; set; }
        public string InvoiceNoteAdmin { get; set; }
        public virtual ICollection<TblInvoiceBillingAddress> BillingAddress { get; set; }
        public virtual ICollection<TblInvoiceDetails> InvoiceDetails { get; set; }
    }
}
