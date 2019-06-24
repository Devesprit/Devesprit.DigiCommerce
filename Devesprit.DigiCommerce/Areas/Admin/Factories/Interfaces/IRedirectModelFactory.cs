using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IRedirectModelFactory
    {
        RedirectModel PrepareRedirectModel(TblRedirects rule);
        TblRedirects PrepareTblRedirects(RedirectModel rule);
    }
}