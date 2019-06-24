using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IProductDiscountsForUserGroupsModelFactory
    {
        ProductDiscountsForUserGroupsModel PrepareProductDiscountsForUserGroupsModel(TblProductDiscountsForUserGroups record, int productId);
        TblProductDiscountsForUserGroups PrepareTblProductDiscountsForUserGroups(ProductDiscountsForUserGroupsModel model);
    }
}