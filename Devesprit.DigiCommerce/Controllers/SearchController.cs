using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models;
using Devesprit.DigiCommerce.Models.Search;
using Devesprit.Services;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Elmah;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce.Controllers
{
    [Intercept(typeof(MethodCache))]
    public partial class SearchController : BaseController
    {
        private readonly IPostService<TblPosts> _postService;
        private readonly IPostModelFactory _postModelFactory;
        private readonly ISearchEngine _searchEngine;
        
        public SearchController(
            IPostService<TblPosts> postService,
            IPostModelFactory postModelFactory,
            ISearchEngine searchEngine)
        {
            _postService = postService;
            _postModelFactory = postModelFactory;
            _searchEngine = searchEngine;
        }

        [Route("{lang}/Search", Order = 0)]
        [Route("Search", Order = 1)]
        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        public virtual async Task<ActionResult> Index(SearchTermModel model)
        {
            if (model.Query.IsNullOrWhiteSpace())
            {
                return View();
            }

            if (Request.QueryString.AllKeys.Length == 1 && !string.IsNullOrWhiteSpace(Request.QueryString["Query"]))
            {
                model.SearchPlace = SearchPlace.Title;
            }

            var result = await _searchEngine.SearchAsync(model.Query, model.FilterByCategory, model.LanguageId ?? 0,
                model.PostType, model.SearchPlace, model.OrderBy);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return View("Error");
            }

            model.Query = model.Query.Trim();
            model.PageSize = model.PageSize ?? int.MaxValue;
            model.Page = model.Page ?? 1;

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(result.Documents.Select(p => p.DocumentId).ToList(), model.Page.Value,
                model.PageSize.Value);

            var viewModel = new SearchResultModel
            {
                TimeElapsed = result.ElapsedMilliseconds,
                SearchTerm = model,
                NumberOfItemsFound = posts.Count,
                SearchResult = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url),
                SuggestSimilar = result.SuggestSimilar,
                CardViewStyles = ViewStyles.Normal
            };

            foreach (var document in result.Documents)
            {
                var post = viewModel.SearchResult.FirstOrDefault(p => p.Id == document.DocumentId);
                if (post != null)
                {
                    post.DescriptionTruncated = document.DocumentBody;
                    post.Title = document.DocumentTitle;
                }
            }

            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            ViewBag.MetaDescription = string.Format(localizationService.GetResource("SearchFor"), model.Query).Replace("\"", "'");
            return View(viewModel);
        }

        [Route("{lang}/Tags/{tag}", Order = 0)]
        [Route("Tags/{tag}", Order = 1)]
        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        public virtual async Task<ActionResult> Tag(string tag, int? page)
        {
            if (tag.IsNullOrWhiteSpace())
            {
                return View("Index");
            }

            var result = await _searchEngine.SearchAsync($"\"{tag}\"", null, 0, null, SearchPlace.Tags,
                SearchResultSortType.Score, 1000, true);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return View("Error");
            }
            
            var searchTerm = new SearchTermModel { Query = tag.Trim(), Page = page ?? 1, PageSize = 20 };

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(result.Documents.Select(p => p.DocumentId).ToList(),
                searchTerm.Page.Value, searchTerm.PageSize.Value);

            var viewModel = new SearchResultModel
            {
                TimeElapsed = result.ElapsedMilliseconds,
                SearchTerm = searchTerm,
                NumberOfItemsFound = posts.Count,
                SearchResult = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url),
                ShowAdvancedSearchPanel = false,
                CardViewStyles = ViewStyles.Small,

            };

            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            ViewBag.MetaDescription = string.Format(localizationService.GetResource("SearchForTag"), tag).Replace("\"", "'");
            return View("Index", viewModel);
        }

        [Route("{lang}/Keywords/{keyword}", Order = 0)]
        [Route("Keywords/{keyword}", Order = 1)]
        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        public virtual async Task<ActionResult> Keyword(string keyword, int? page)
        {
            if (keyword.IsNullOrWhiteSpace())
            {
                return View("Index");
            }

            var result = await _searchEngine.SearchAsync($"\"{keyword}\"", null, 0, null, SearchPlace.Keywords,
                SearchResultSortType.Score, 1000, true);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return View("Error");
            }

            var searchTerm = new SearchTermModel { Query = keyword.Trim(), Page = page ?? 1, PageSize = 20 };

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(result.Documents.Select(p => p.DocumentId).ToList(),
                searchTerm.Page.Value, searchTerm.PageSize.Value);
             
            var viewModel = new SearchResultModel
            {
                TimeElapsed = result.ElapsedMilliseconds,
                SearchTerm = searchTerm,
                NumberOfItemsFound = posts.Count,
                SearchResult = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url),
                ShowAdvancedSearchPanel = false,
                CardViewStyles = ViewStyles.Small,
            };

            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            ViewBag.MetaDescription = string.Format(localizationService.GetResource("SearchForKeyword"), keyword).Replace("\"", "'");
            return View("Index", viewModel);
        }

        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        public virtual ActionResult MoreLikeThis(int postId, PostType postType, int? numberOfSimilarityPosts)
        {
            var result = _searchEngine.MoreLikeThis(postId, null, 0, postType,
                SearchPlace.Title | SearchPlace.Description | SearchPlace.Tags,
                numberOfSimilarityPosts ?? 20);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return Content("");
            }

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(result.Documents.Select(p => p.DocumentId).Take(5).ToList(), 1,
                numberOfSimilarityPosts ?? 5);
            return PartialView("Partials/_MoreLikeThis", _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url));
        }

        [MethodCache(Tags = new[] { CacheTags.Search }, VaryByCustom = "lang")]
        public virtual async Task<JsonResult> SearchSuggestion(string query)
        {
            var result = await _searchEngine.AutoCompleteAsync(query, 0, 20);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current)
                    .Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return Json("", JsonRequestBehavior.AllowGet);
            }

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(result.Documents.Select(p => p.DocumentId).Take(10).ToList());
            var model = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url);

            return Json(model.Select(p => new {value = p.Title, data = p.PostUrl}).ToList(),
                JsonRequestBehavior.AllowGet);
        }
    }
}