using System;
using Devesprit.Data.Enums;

namespace Devesprit.DigiCommerce.Models.Invoice
{
    public partial class InvoiceDetailsModel
    {
        public int Id { get; set; }
        public Guid InvoiceId { get; set; }
        public int ItemId { get; set; }
        public string ItemHomePage { get; set; }
        public InvoiceDetailsItemType ItemType { get; set; }
        public string ItemName { get; set; }
        public string ItemLicenseCode { get; set; }
        public int Qty { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice => UnitPrice * Qty;
        public DateTime? PurchaseExpiration { get; set; }
        public int RowNumber { get; set; }
    }
}