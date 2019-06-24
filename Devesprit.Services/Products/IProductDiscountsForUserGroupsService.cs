using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Products
{
    public partial interface IProductDiscountsForUserGroupsService
    {
        Task<TblProductDiscountsForUserGroups> FindByIdAsync(int id);
        IQueryable<TblProductDiscountsForUserGroups> GetAsQueryable(int? filterByProductId);
        IEnumerable<TblProductDiscountsForUserGroups> FindProductDiscounts(int productId);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblProductDiscountsForUserGroups record);
        Task<int> AddAsync(TblProductDiscountsForUserGroups record);
    }
}