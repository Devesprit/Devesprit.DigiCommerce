using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Products
{
    public partial interface IProductCheckoutAttributesService
    {
        Task<TblProductCheckoutAttributes> FindByIdAsync(int id);
        IQueryable<TblProductCheckoutAttributes> GetAsQueryable(int? filterByProductId);
        Task<IEnumerable<TblProductCheckoutAttributes>> FindProductAttributesAsync(int productId);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblProductCheckoutAttributes record);
        Task<int> AddAsync(TblProductCheckoutAttributes record);
        Task<double> CalculateAttributeOptionPriceForUserAsync(int optionId, TblUsers user);


        #region Attribute Options

        Task<TblProductCheckoutAttributeOptions> FindOptionByIdAsync(int id);
        IQueryable<TblProductCheckoutAttributeOptions> GetOptionsAsQueryable(int? filterByAttributeId);
        Task DeleteOptionAsync(int id);
        Task UpdateOptionAsync(TblProductCheckoutAttributeOptions record);
        Task<int> AddOptionAsync(TblProductCheckoutAttributeOptions record);

        #endregion
    }
}