using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class PostAttributeMappingModelFactory : IPostAttributeMappingModelFactory
    {
        public virtual async Task<PostAttributeMappingModel> PreparePostAttributeMappingModelAsync(TblPostAttributesMapping attribute, int postId)
        {
            PostAttributeMappingModel result;
            if (attribute == null)
            {
                result = new PostAttributeMappingModel();
            }
            else
            {
                result = Mapper.Map<PostAttributeMappingModel>(attribute);
                await attribute.LoadAllLocalizedStringsToModelAsync(result);
            }

            result.PostId = postId;
            return result;
        }

        public virtual TblPostAttributesMapping PrepareTblPostAttributesMapping(PostAttributeMappingModel attribute)
        {
            var result = Mapper.Map<TblPostAttributesMapping>(attribute);
            return result;
        }
    }
}