using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.Services.Blog;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Posts;
using Microsoft.AspNet.Identity;
using X.PagedList;

namespace Devesprit.DigiCommerce.Controllers
{
    [Intercept(typeof(MethodCache))]
    public partial class BlogController : BaseController
    {
        private readonly IBlogPostService _blogPostService;
        private readonly IPostModelFactory _postModelFactory;
        private readonly IPostCategoriesService _categoriesService;

        public BlogController(
            IBlogPostService blogPostService,
            IPostModelFactory postModelFactory,
            IPostCategoriesService categoriesService)
        {
            _blogPostService = blogPostService;
            _postModelFactory = postModelFactory;
            _categoriesService = categoriesService;
        }

        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        public virtual ActionResult Index()
        {
            return View();
        }

        [Route("{lang}/Blog/Post/{id}/{slug}", Order = 0)]
        [Route("Blog/Post/{id}/{slug}", Order = 1)]
        [Route("{lang}/Blog/Post/{slug}", Order = 2)]
        [Route("Blog/Post/{slug}", Order = 3)]
        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang" /*"lang,user"*/)]
        public virtual async Task<ActionResult> Post(int? id, string slug)
        {
            if (!CurrentSettings.EnableBlog)
            {
                return View("PageNotFound");
            }

            var currentUser = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            var isAdmin = HttpContext.User.IsInRole("Admin");

            TblBlogPosts post = null;
            if (id != null)
            {
                post = await _blogPostService.FindByIdAsync(id.Value);
            }

            if (post == null)
            {
                post = await _blogPostService.FindBySlugAsync(slug);
            }

            if (post == null && int.TryParse(slug, out int postId))
            {
                post = await _blogPostService.FindByIdAsync(postId);
            }

            if (post == null || (!post.Published && !isAdmin))
            {
                return View("PageNotFound");
            }

            //Increase the number of post views
            await _blogPostService.IncreaseNumberOfViewsAsync(post);

            //Current post editor page URL (for Admin User)
            ViewBag.AdminEditCurrentPage =
                $"PopupWindows('{Url.Action("Editor", "ManageBlogPosts", new { area = "Admin" })}', 'BlogPostEditor', 1200, 700, {{ id: {post.Id} }}, 'get')";

            return View(_postModelFactory.PreparePostModel(post, currentUser, Url));
        }

        [Route("{lang}/Blog/{listType}", Order = 0)]
        [Route("Blog/{listType}", Order = 1)]
        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        public virtual ActionResult BlogExplorer(PostsListType listType, int? page, int? pageSize, int? catId, DateTime? fromDate)
        {
            if (!CurrentSettings.EnableBlog)
            {
                return View("PageNotFound");
            }

            return View(new PostsExplorerModel()
            {
                PageIndex = page ?? 1,
                PageSize = pageSize,
                PostsListType = listType,
                FilterByCategoryId = catId,
                FromDate = fromDate
            });
        }

        [ChildActionOnly]
        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        public virtual ActionResult GetBlogPostsList(PostsListType listType, int? page, int? pageSize, int? catId, DateTime? fromDate, ViewStyles? style, NumberOfCol? numberOfCol, bool? showPager)
        {
            if (!CurrentSettings.EnableBlog)
            {
                return View("PageNotFound");
            }

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            IPagedList<TblBlogPosts> posts = null;
            switch (listType)
            {
                case PostsListType.Newest:
                    posts = _blogPostService.GetNewItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case PostsListType.MostPopular:
                    posts = _blogPostService.GetPopularItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case PostsListType.HotList:
                    posts = _blogPostService.GetHotList(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case PostsListType.Featured:
                    posts = _blogPostService.GetFeaturedItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
            }
            var model = new PostsListModel()
            {
                PostsList = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url),
                ViewStyle = style ?? ViewStyles.Normal,
                PageIndex = page ?? 1,
                PageSize = pageSize,
                PostsListType = listType,
                FromDate = fromDate,
                FilterByCategoryId = catId,
                ShowPager = showPager ?? true,
                NumberOfCol = numberOfCol ?? NumberOfCol.Four
            };

            return View("Partials/_PostsList", model);
        }

        [Route("{lang}/BlogCategories/{slug}", Order = 0)]
        [Route("BlogCategories/{slug}", Order = 1)]
        [MethodCache(Tags = new[] { nameof(TblBlogPosts) }, VaryByCustom = "lang")]
        public virtual async Task<ActionResult> FilterByCategory(string slug, int? page, int? pageSize)
        {
            if (!CurrentSettings.EnableBlog)
            {
                return View("PageNotFound");
            }

            var category = await _categoriesService.FindBySlugAsync(slug);
            if (category == null && int.TryParse(slug, out int categoryId))
            {
                category = await _categoriesService.FindByIdAsync(categoryId);
            }

            if (category == null || (category.DisplayArea != DisplayArea.BlogSection && category.DisplayArea != DisplayArea.Both))
            {
                return View("PageNotFound");
            }

            return View(new PostsExplorerModel()
            {
                CategoryName = category.GetLocalized(p => p.CategoryName),
                FilterByCategoryId = category.Id,
                PageIndex = page ?? 1,
                PageSize = pageSize
            });
        }
    }
}