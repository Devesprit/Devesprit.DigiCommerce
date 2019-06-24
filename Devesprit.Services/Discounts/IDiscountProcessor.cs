using Devesprit.Core.Plugin;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Discounts
{
    public partial interface IDiscountProcessor : IPlugin
    {
        DiscountProcessorResult ProcessorInvoice(TblInvoices invoice);
        int Order { get; }
    }
}
