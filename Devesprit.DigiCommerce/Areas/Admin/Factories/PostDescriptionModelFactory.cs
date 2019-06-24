using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class PostDescriptionModelFactory : IPostDescriptionModelFactory
    {
        public virtual async Task<PostDescriptionModel> PreparePostDescriptionModelAsync(TblPostDescriptions description, int postId)
        {
            PostDescriptionModel result;
            if (description == null)
            {
                result = new PostDescriptionModel();
            }
            else
            {
                result = Mapper.Map<PostDescriptionModel>(description);
                await description.LoadAllLocalizedStringsToModelAsync(result);
            }

            result.PostId = postId;
            return result;
        }

        public virtual TblPostDescriptions PrepareTblPostDescriptions(PostDescriptionModel description)
        {
            var result = Mapper.Map<TblPostDescriptions>(description);
            return result;
        }
    }
}