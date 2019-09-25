using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class PageModelFactory : IPageModelFactory
    {
        public virtual async Task<PageModel> PreparePageModelAsync(TblPages page)
        {
            PageModel result;
            if (page == null)
            {
                result = new PageModel();
            }
            else
            {
                result = page.Adapt<PageModel>();
                await page.LoadAllLocalizedStringsToModelAsync(result);
            }
            return result;
        }

        public virtual TblPages PrepareTblPages(PageModel page)
        {
            var result = page.Adapt<TblPages>();
            return result;
        }
    }
}