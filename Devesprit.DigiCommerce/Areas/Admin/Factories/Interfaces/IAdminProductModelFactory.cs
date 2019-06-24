using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IAdminProductModelFactory
    {
        Task<ProductModel> PrepareProductModelAsync(TblProducts product);
        TblProducts PrepareTblProducts(ProductModel product);
    }
}
