using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.Users;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Blog
{
    public partial class BlogPostService : PostService<TblBlogPosts>, IBlogPostService
    {
        public BlogPostService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService,
            IPostCategoriesService categoriesService,
            IEventPublisher eventPublisher): base(dbContext,
            localizedEntityService,
            userLikesService,
            userWishlistService,
            categoriesService,
            eventPublisher)
        {}
    }
}