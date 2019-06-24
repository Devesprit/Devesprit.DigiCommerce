using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IPostAttributesModelFactory
    {
        Task<PostAttributeModel> PreparePostAttributeModelAsync(TblPostAttributes attribute);
        TblPostAttributes PrepareTblPostAttributes(PostAttributeModel attribute);


        #region Post Attribute Options

        Task<PostAttributeOptionModel> PreparePostAttributeOptionModelAsync(TblPostAttributeOptions option, int attributeId);
        TblPostAttributeOptions PrepareTblPostAttributeOptions(PostAttributeOptionModel option);

        #endregion
    }
}
