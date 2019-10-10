using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models.Products;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories.Interfaces
{
    public partial interface IProductModelFactory
    {
        IPagedList<ProductCardViewModel> PrepareProductCardViewModel(IPagedList<TblProducts> product, TblUsers currentUser, UrlHelper url);
        ProductModel PrepareProductModel(TblProducts product, TblUsers currentUser, UrlHelper url);
        ProductDownloadModel PrepareProductDownloadPurchaseButtonModel(TblProducts product, TblUsers currentUser);
    }
}