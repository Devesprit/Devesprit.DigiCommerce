using System.Threading.Tasks;
using System.Web.Routing;
using Devesprit.Core.Plugin;
using Devesprit.Data.Domain;
using Devesprit.Services.LicenseManager;
using Devesprit.Services.Localization;

namespace Plugin.LicenseGenerator.Sample
{
    public partial class SampleLicenseGeneratorPlugin : BasePlugin, ILicenseGenerator
    {
        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "";
            controllerName = "";
            routeValues = new RouteValueDictionary();
        }
        
        public string LicenseGeneratorServiceId => "Sample License Generator";
        public virtual async Task<string> GenerateLicenseForProductAsync(TblInvoices invoice, TblUsers user,
            TblProducts product, int quantity)
        {
            return "Test product license for : " + product.GetLocalized(p=> p.Title);
        }

        public virtual async Task<string> GenerateLicenseForProductAttributeAsync(TblInvoices invoice, TblUsers user, TblProductCheckoutAttributes attribute, int quantity)
        {
            return "Test product checkout attribute license for : " + attribute.GetLocalized(p=> p.Name);
        }

        public virtual async Task<string> GenerateLicenseForProductAttributeOptionAsync(TblInvoices invoice, TblUsers user,
            TblProductCheckoutAttributeOptions attributeOption, int quantity)
        {
            return "Test product checkout attribute option license for : " + attributeOption.GetLocalized(p=> p.Name);
        }
    }
}
