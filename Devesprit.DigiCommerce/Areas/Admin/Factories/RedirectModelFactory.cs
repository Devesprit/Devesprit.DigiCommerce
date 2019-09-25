using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class RedirectModelFactory : IRedirectModelFactory
    {
        public virtual RedirectModel PrepareRedirectModel(TblRedirects rule)
        {
            var result = rule == null ? new RedirectModel() : rule.Adapt<RedirectModel>();
            return result;
        }

        public virtual TblRedirects PrepareTblRedirects(RedirectModel rule)
        {
            var result = rule.Adapt<TblRedirects>();
            result.Name = string.IsNullOrWhiteSpace(rule.Name) ? rule.RequestedUrl : rule.Name;
            return result;
        }
    }
}