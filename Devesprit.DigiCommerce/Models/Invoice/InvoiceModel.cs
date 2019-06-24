using System;
using System.Collections.Generic;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.PaymentGateway;

namespace Devesprit.DigiCommerce.Models.Invoice
{
    public partial class InvoiceModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double DiscountAmount { get; set; }
        public string DiscountDescription { get; set; }
        public double TotalTaxAmount { get; set; }
        public string TaxDescription { get; set; }
        public int? CurrencyId { get; set; }
        public TblCurrencies Currency { get; set; }
        public InvoiceStatus Status { get; set; }
        public string PaymentGatewayName { get; set; }
        public string PaymentGatewaySystemName { get; set; }
        public string PaymentGatewayTransactionId { get; set; }
        public string InvoiceNote { get; set; }
        public string InvoiceNoteAdmin { get; set; }
        public InvoiceBillingAddressModel UserBillingAddress { get; set; }
        public List<InvoiceDetailsModel> InvoiceDetails { get; set; }

        public double InvoiceSubTotal { get; set; }
        public double InvoiceTotal { get; set; }
        public List<IPaymentMethod> PaymentGateways { get; set; }
    }
}