using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Services.Posts;
using Devesprit.Services.Users;

namespace Devesprit.Services.Blog
{
    public partial class BlogPostService : PostService<TblBlogPosts>, IBlogPostService
    {
        public BlogPostService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService,
            IPostCategoriesService categoriesService,
            IEventPublisher eventPublisher) : base(dbContext,
            localizedEntityService,
            userLikesService,
            userWishlistService,
            categoriesService,
            eventPublisher)
        { }
    }
}