using System;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Users
{
    public partial class UserPurchasedProductAttribute
    {
        public TblUsers User { get; set; }
        public string Name { get; set; }
        public string HomePage { get; set; }
        public Guid InvoiceId { get; set; }
        public int InvoiceDetailsId { get; set; }
        public int AttributeOptionId { get; set; }
        public int AttributeId { get; set; }
        public TblProductCheckoutAttributeOptions Option { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime PurchaseExpiration { get; set; }
    }
}