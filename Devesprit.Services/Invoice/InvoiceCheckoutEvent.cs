using Devesprit.Data.Domain;
using Devesprit.Data.Events;

namespace Devesprit.Services.Invoice
{
    public partial class InvoiceCheckoutEvent: IEvent
    {
        public TblInvoices Invoice { get; }
        public string TransactionId { get; }
        public double PaidAmount { get; }

        public InvoiceCheckoutEvent(TblInvoices invoice, string transactionId, double paidAmount)
        {
            Invoice = invoice;
            TransactionId = transactionId;
            PaidAmount = paidAmount;
        }
    }
}