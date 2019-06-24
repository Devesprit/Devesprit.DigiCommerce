using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Posts
{
    public partial interface IPostAttributesService
    {
        IQueryable<TblPostAttributes> GetAsQueryable();
        Task<TblPostAttributes> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPostAttributes record);
        Task<int> AddAsync(TblPostAttributes record);
        List<SelectListItem> GetAsSelectList();


        #region Attribute Options

        IQueryable<TblPostAttributeOptions> GetOptionsAsQueryable(int? filterByAttributeId);
        Task<TblPostAttributeOptions> FindOptionByIdAsync(int id);
        Task DeleteOptionAsync(int id);
        Task UpdateOptionAsync(TblPostAttributeOptions record);
        Task<int> AddOptionAsync(TblPostAttributeOptions record);
        List<SelectListItem> GetOptionsAsSelectList(int attributeId);

        #endregion
    }
}