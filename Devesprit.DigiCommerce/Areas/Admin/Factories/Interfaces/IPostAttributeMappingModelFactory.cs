using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IPostAttributeMappingModelFactory
    {
        Task<PostAttributeMappingModel> PreparePostAttributeMappingModelAsync(TblPostAttributesMapping attribute, int postId);
        TblPostAttributesMapping PrepareTblPostAttributesMapping(PostAttributeMappingModel attribute);
    }
}