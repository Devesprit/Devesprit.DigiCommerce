using System;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Users
{
    public partial class UserPurchasedProduct
    {
        public TblUsers User { get; set; }
        public string ProductName { get; set; }
        public string ProductHomePage { get; set; }
        public Guid InvoiceId { get; set; }
        public int InvoiceDetailsId { get; set; }
        public int ProductId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime PurchaseExpiration { get; set; }
    }
}