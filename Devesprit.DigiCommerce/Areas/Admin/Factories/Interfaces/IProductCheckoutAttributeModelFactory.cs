using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IProductCheckoutAttributeModelFactory
    {
        Task<ProductCheckoutAttributeModel> PrepareProductCheckoutAttributeModelAsync(TblProductCheckoutAttributes attribute, int productId);
        TblProductCheckoutAttributes PrepareTblProductCheckoutAttributes(ProductCheckoutAttributeModel attribute);


        #region Product Checkout Attribute Options

        Task<ProductCheckoutAttributeOptionModel> PrepareProductCheckoutAttributeOptionModelAsync(TblProductCheckoutAttributeOptions option, int attributeId);
        TblProductCheckoutAttributeOptions PrepareTblProductCheckoutAttributeOptions(ProductCheckoutAttributeOptionModel option);

        #endregion
    }
}