using System.Collections.Generic;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models.Invoice;

namespace Devesprit.DigiCommerce.Factories.Interfaces
{
    public partial interface IInvoiceModelFactory
    {
        Task<InvoiceModel> PrepareInvoiceModelAsync(TblInvoices invoice);
        List<InvoiceDetailsModel> PrepareInvoiceDetailsModel(ICollection<TblInvoiceDetails> details);
        InvoiceBillingAddressModel PrepareInvoiceBillingAddressModel(TblInvoiceBillingAddress address);
        TblInvoiceBillingAddress PrepareTblInvoiceBillingAddress(InvoiceBillingAddressModel address);
    }
}