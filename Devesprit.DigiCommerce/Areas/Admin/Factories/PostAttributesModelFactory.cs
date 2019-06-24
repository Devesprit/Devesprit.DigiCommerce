using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;

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
                result = Mapper.Map<PostAttributeModel>(attribute);
                await attribute.LoadAllLocalizedStringsToModelAsync(result);
            }

            return result;
        }

        public virtual TblPostAttributes PrepareTblPostAttributes(PostAttributeModel attribute)
        {
            var result = Mapper.Map<TblPostAttributes>(attribute);
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
                result = Mapper.Map<PostAttributeOptionModel>(option);
                await option.LoadAllLocalizedStringsToModelAsync(result);
            }

            result.PostAttributeId = attributeId;
            return result;
        }

        public virtual TblPostAttributeOptions PrepareTblPostAttributeOptions(PostAttributeOptionModel option)
        {
            var result = Mapper.Map<TblPostAttributeOptions>(option);
            return result;
        }

        #endregion
    }
}