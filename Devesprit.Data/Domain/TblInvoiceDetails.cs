using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_InvoiceDetails")]
    public partial class TblInvoiceDetails:BaseEntity
    {
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public Guid InvoiceId { get; set; }
        public virtual  TblInvoices Invoice { get; set; }
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public int ItemId { get; set; }
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public InvoiceDetailsItemType ItemType { get; set; }
        public string ItemName { get; set; }
        public string ItemHomePage { get; set; }
        public int Qty { get; set; }
        public double UnitPrice { get; set; }
        public DateTime? PurchaseExpiration { get; set; }
        public string ItemLicenseCode { get; set; }
    }
}