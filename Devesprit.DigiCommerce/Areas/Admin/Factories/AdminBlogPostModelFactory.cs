using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;
using X.PagedList;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class AdminBlogPostModelFactory : IAdminBlogPostModelFactory
    {
        public virtual async Task<BlogPostModel> PrepareBlogPostModelAsync(TblBlogPosts blogPost)
        {
            BlogPostModel result;
            if (blogPost == null)
            {
                result = new BlogPostModel();
            }
            else
            {
                result = blogPost.Adapt<BlogPostModel>();
                await blogPost.LoadAllLocalizedStringsToModelAsync(result);
                
                var tags = await blogPost.Tags.Select(p => p.Tag).OrderBy(p => p).ToListAsync();
                result.PostTags = tags?.ToArray() ?? new string[] { };

                var categories = await blogPost.Categories.OrderBy(p => p.CategoryName).Select(p => p.Id).ToListAsync();
                result.PostCategories = categories.ToArray();
            }

            return result;
        }

        public virtual TblBlogPosts PrepareTblBlogPosts(BlogPostModel post)
        {
            var result = post.Adapt<TblBlogPosts>();
            result.Tags = post.PostTags?.Select(p => new TblPostTags() { Tag = p }).ToList();
            result.Categories =
                post.PostCategories?.Select(p => new TblPostCategories() { Id = p }).ToList();

            return result;
        }

    }
}