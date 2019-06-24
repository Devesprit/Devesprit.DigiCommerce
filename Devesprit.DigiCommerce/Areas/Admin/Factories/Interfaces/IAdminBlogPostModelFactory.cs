using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IAdminBlogPostModelFactory
    {
        Task<BlogPostModel> PrepareBlogPostModelAsync(TblBlogPosts blogPost);
        TblBlogPosts PrepareTblBlogPosts(BlogPostModel product);
    }
}
