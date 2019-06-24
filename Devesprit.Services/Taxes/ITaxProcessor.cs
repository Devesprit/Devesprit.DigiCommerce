using Devesprit.Core.Plugin;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Taxes
{
    public partial interface ITaxProcessor : IPlugin
    {
        TaxProcessorResult ProcessorInvoice(TblInvoices invoice);
        int Order { get; }
    }
}
