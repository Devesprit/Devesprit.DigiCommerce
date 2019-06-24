using System.Linq;
using Devesprit.Data.Domain;
using Devesprit.Services.Currency;

namespace Devesprit.Services.Invoice
{
    public static partial class InvoiceExtensions
    {
        public static double ComputeInvoiceTotalAmount(this TblInvoices invoice, bool exchangeCurrency = true, bool withDiscountAndTax = true)
        {
            double sum = 0;
            if (withDiscountAndTax)
            {
                sum = (invoice.InvoiceDetails?.Sum(p => p.UnitPrice * p.Qty) ?? 0) - (invoice.DiscountAmount ?? 0) + (invoice.TotalTaxAmount ?? 0);
            }
            else
            {
                sum = invoice.InvoiceDetails?.Sum(p => p.UnitPrice * p.Qty) ?? 0;
            }
            sum = sum < 0 ? 0 : sum;
            return exchangeCurrency ? sum.ExchangeCurrency() : sum;
        }
    }
}
