using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Devesprit.Services.Posts;
using Devesprit.Services.Products;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class PostCategoriesModelFactory : IPostCategoriesModelFactory
    {
        private readonly IPostCategoriesService _postCategoriesService;

        public PostCategoriesModelFactory(IPostCategoriesService postCategoriesService)
        {
            _postCategoriesService = postCategoriesService;
        }

        public virtual async Task<PostCategoryModel> PreparePostCategoryModelAsync(TblPostCategories category)
        {
            PostCategoryModel result;
            if (category == null)
            {
                result = new PostCategoryModel();
            }
            else
            {
                result = Mapper.Map<PostCategoryModel>(category);
                await category.LoadAllLocalizedStringsToModelAsync(result);
            }
            result.CategoriesList = await _postCategoriesService.GetAsSelectListAsync();
            return result;
        }

        public virtual TblPostCategories PrepareTblPostCategories(PostCategoryModel category)
        {
            var result = Mapper.Map<TblPostCategories>(category);
            return result;
        }
    }
}