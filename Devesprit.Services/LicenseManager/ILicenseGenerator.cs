using System.Threading.Tasks;
using Devesprit.Core.Plugin;
using Devesprit.Data.Domain;

namespace Devesprit.Services.LicenseManager
{
    public partial interface ILicenseGenerator: IPlugin
    {
        string LicenseGeneratorServiceId { get; }
        Task<string> GenerateLicenseForProductAsync(TblInvoices invoice, TblUsers user, TblProducts product, int quantity);
        Task<string> GenerateLicenseForProductAttributeAsync(TblInvoices invoice, TblUsers user, TblProductCheckoutAttributes attribute, int quantity);
        Task<string> GenerateLicenseForProductAttributeOptionAsync(TblInvoices invoice, TblUsers user, TblProductCheckoutAttributeOptions attributeOption, int quantity);
    }
}
