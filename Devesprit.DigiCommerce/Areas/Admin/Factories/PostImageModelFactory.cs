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
    public partial class PostImageModelFactory : IPostImageModelFactory
    {
        private readonly IPostService<TblPosts> _postService;

        public PostImageModelFactory(IPostService<TblPosts> postService)
        {
            _postService = postService;
        }

        public virtual async Task<PostImageModel> PreparePostImageModelAsync(TblPostImages image, int postId)
        {
            PostImageModel result;
            if (image == null)
            {
                result = new PostImageModel();
            }
            else
            {
                result = Mapper.Map<PostImageModel>(image);
                result.PostId = image.PostId;
                await image.LoadAllLocalizedStringsToModelAsync(result);
            }

            result.PostId = postId;
            return result;
        }

        public virtual async Task<TblPostImages> PrepareTblPostImagesAsync(PostImageModel image)
        {
            var result = Mapper.Map<TblPostImages>(image);
            result.PostId = image.PostId;
            if (string.IsNullOrWhiteSpace(result.Alt))
            {
                var post = await _postService.FindByIdAsync(image.PostId);
                result.Alt = post.GetLocalized(p=> p.Title);
            }
            return result;
        }
    }
}