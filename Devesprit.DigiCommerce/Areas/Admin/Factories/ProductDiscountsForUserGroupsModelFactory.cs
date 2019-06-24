using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class ProductDiscountsForUserGroupsModelFactory : IProductDiscountsForUserGroupsModelFactory
    {
        public virtual ProductDiscountsForUserGroupsModel PrepareProductDiscountsForUserGroupsModel(TblProductDiscountsForUserGroups record, int productId)
        {
            var result = record == null
                ? new ProductDiscountsForUserGroupsModel()
                : Mapper.Map<ProductDiscountsForUserGroupsModel>(record);

            result.ProductId = productId;
            return result;
        }

        public virtual TblProductDiscountsForUserGroups PrepareTblProductDiscountsForUserGroups(ProductDiscountsForUserGroupsModel model)
        {
            var result = Mapper.Map<TblProductDiscountsForUserGroups>(model);
            return result;
        }
    }
}