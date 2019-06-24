using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models;
using Devesprit.DigiCommerce.Models.Products;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories.Interfaces
{
    public partial interface IProductModelFactory
    {
        ProductCardViewModel PrepareProductCardViewModel(TblProducts product, TblUsers currentUser, UrlHelper url);
        IPagedList<ProductCardViewModel> PrepareProductCardViewModel(IPagedList<TblProducts> product, TblUsers currentUser, UrlHelper url);
        ProductModel PrepareProductModel(TblProducts product, TblUsers currentUser, UrlHelper url);
        ProductDownloadPurchaseButtonModel PrepareProductDownloadPurchaseButtonModel(TblProducts product, TblUsers currentUser);
    }
}