using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class ProductCheckoutAttributeModelFactory : IProductCheckoutAttributeModelFactory
    {
        public virtual async Task<ProductCheckoutAttributeModel> PrepareProductCheckoutAttributeModelAsync(TblProductCheckoutAttributes attribute, int productId)
        {
            ProductCheckoutAttributeModel result;
            if (attribute == null)
            {
                result = new ProductCheckoutAttributeModel();
            }
            else
            {
                result = attribute.Adapt<ProductCheckoutAttributeModel>();
                await attribute.LoadAllLocalizedStringsToModelAsync(result);
            }

            result.ProductId = productId;
            return result;
        }

        public virtual TblProductCheckoutAttributes PrepareTblProductCheckoutAttributes(ProductCheckoutAttributeModel attribute)
        {
            var result = attribute.Adapt<TblProductCheckoutAttributes>();
            return result;
        }


        #region Product Checkout Attributes

        public virtual async Task<ProductCheckoutAttributeOptionModel> PrepareProductCheckoutAttributeOptionModelAsync(TblProductCheckoutAttributeOptions option, int attributeId)
        {
            ProductCheckoutAttributeOptionModel result;
            if (option == null)
            {
                result = new ProductCheckoutAttributeOptionModel();
            }
            else
            {
                result = option.Adapt<ProductCheckoutAttributeOptionModel>();
                await option.LoadAllLocalizedStringsToModelAsync(result);
            }

            result.ProductCheckoutAttributeId = attributeId;
            return result;
        }

        public virtual TblProductCheckoutAttributeOptions PrepareTblProductCheckoutAttributeOptions(ProductCheckoutAttributeOptionModel option)
        {
            var result = option.Adapt<TblProductCheckoutAttributeOptions>();
            return result;
        }

        #endregion
    }
}