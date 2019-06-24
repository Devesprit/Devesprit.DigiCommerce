using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class RedirectModelFactory : IRedirectModelFactory
    {
        public virtual RedirectModel PrepareRedirectModel(TblRedirects rule)
        {
            var result = rule == null ? new RedirectModel() : Mapper.Map<RedirectModel>(rule);
            return result;
        }

        public virtual TblRedirects PrepareTblRedirects(RedirectModel rule)
        {
            var result = Mapper.Map<TblRedirects>(rule);
            result.Name = string.IsNullOrWhiteSpace(rule.Name) ? rule.RequestedUrl : rule.Name;
            return result;
        }
    }
}