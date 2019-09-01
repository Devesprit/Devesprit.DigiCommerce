using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Invoice
{
    public partial class InvoiceCheckoutEvent: IEvent
    {
        public TblInvoices Invoice { get; }
        public string TransactionId { get; }
        public double PaidAmount { get; }
        public string PaidAmountExStr { get; }
        public string PaymentGatewayName { get; set; }
        public string CurrencyIso { get; set; }

        public InvoiceCheckoutEvent(TblInvoices invoice, string transactionId, double paidAmount, string paidAmountExStr, string paymentGatewayName, string currencyIso)
        {
            Invoice = invoice;
            TransactionId = transactionId;
            PaidAmount = paidAmount;
            PaidAmountExStr = paidAmountExStr;
            PaymentGatewayName = paymentGatewayName;
            CurrencyIso = currencyIso;
        }
    }
}