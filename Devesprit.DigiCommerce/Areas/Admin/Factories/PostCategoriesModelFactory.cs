using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Devesprit.Services.Posts;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class PostCategoriesModelFactory : IPostCategoriesModelFactory
    {
        public virtual async Task<PostCategoryModel> PreparePostCategoryModelAsync(TblPostCategories category)
        {
            PostCategoryModel result;
            if (category == null)
            {
                result = new PostCategoryModel();
            }
            else
            {
                result = category.Adapt<PostCategoryModel>();
                await category.LoadAllLocalizedStringsToModelAsync(result);
            }
            return result;
        }

        public virtual TblPostCategories PrepareTblPostCategories(PostCategoryModel category)
        {
            var result = category.Adapt<TblPostCategories>();
            return result;
        }
    }
}