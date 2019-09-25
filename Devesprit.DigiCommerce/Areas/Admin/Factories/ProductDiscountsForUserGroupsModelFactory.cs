using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class ProductDiscountsForUserGroupsModelFactory : IProductDiscountsForUserGroupsModelFactory
    {
        public virtual ProductDiscountsForUserGroupsModel PrepareProductDiscountsForUserGroupsModel(TblProductDiscountsForUserGroups record, int productId)
        {
            var result = record == null
                ? new ProductDiscountsForUserGroupsModel()
                : record.Adapt<ProductDiscountsForUserGroupsModel>();

            result.ProductId = productId;
            return result;
        }

        public virtual TblProductDiscountsForUserGroups PrepareTblProductDiscountsForUserGroups(ProductDiscountsForUserGroupsModel model)
        {
            var result = model.Adapt<TblProductDiscountsForUserGroups>();
            return result;
        }
    }
}