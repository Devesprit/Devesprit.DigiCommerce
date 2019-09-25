using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class PostAttributesModelFactory : IPostAttributesModelFactory
    {
        public virtual async Task<PostAttributeModel> PreparePostAttributeModelAsync(TblPostAttributes attribute)
        {
            PostAttributeModel result;
            if (attribute == null)
            {
                result = new PostAttributeModel();
            }
            else
            {
                result = attribute.Adapt<PostAttributeModel>();
                await attribute.LoadAllLocalizedStringsToModelAsync(result);
            }

            return result;
        }

        public virtual TblPostAttributes PrepareTblPostAttributes(PostAttributeModel attribute)
        {
            var result = attribute.Adapt<TblPostAttributes>();
            return result;
        }
        


        #region Post Attribute Options

        public virtual async Task<PostAttributeOptionModel> PreparePostAttributeOptionModelAsync(TblPostAttributeOptions option, int attributeId)
        {
            PostAttributeOptionModel result;
            if (option == null)
            {
                result = new PostAttributeOptionModel();
            }
            else
            {
                result = option.Adapt<PostAttributeOptionModel>();
                await option.LoadAllLocalizedStringsToModelAsync(result);
            }

            result.PostAttributeId = attributeId;
            return result;
        }

        public virtual TblPostAttributeOptions PrepareTblPostAttributeOptions(PostAttributeOptionModel option)
        {
            var result = option.Adapt<TblPostAttributeOptions>();
            return result;
        }

        #endregion
    }
}