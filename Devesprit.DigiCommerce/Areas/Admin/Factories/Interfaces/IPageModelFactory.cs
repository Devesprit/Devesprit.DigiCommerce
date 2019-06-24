using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IPageModelFactory
    {
        Task<PageModel> PreparePageModelAsync(TblPages page);
        TblPages PrepareTblPages(PageModel page);
    }
}