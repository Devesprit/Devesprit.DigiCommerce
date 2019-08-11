using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models;
using Devesprit.DigiCommerce.Models.Search;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Elmah;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce.Controllers
{
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
        public virtual async Task<ActionResult> Index(SearchTermModel model)
        {
            if (model.Query.IsNullOrWhiteSpace())
            {
                return View();
            }

            model.LanguageId = model.LanguageId ?? WorkContext.CurrentLanguage.Id;

            var result = await _searchEngine.SearchAsync(model.Query, model.FilterByCategory, model.LanguageId.Value, model.PostType, model.SearchPlace);
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
            var posts = _postService.GetItemsById(
                result.Documents.OrderByDescending(p => p.Score).Select(p => p.DocumentId).ToList(), model.Page.Value,
                model.PageSize.Value, model.OrderBy);

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

            return View(viewModel);
        }


        [Route("{lang}/Tags/{tag}", Order = 0)]
        [Route("Tags/{tag}", Order = 1)]
        public virtual async Task<ActionResult> Tag(string tag, int? page)
        {
            if (tag.IsNullOrWhiteSpace())
            {
                return View("Index");
            }

            var result = await _searchEngine.SearchAsync($"\"{tag}\"", null, LanguagesService.GetDefaultLanguage().Id, null, SearchPlace.Tags);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return View("Error");
            }
            
            var searchTerm = new SearchTermModel { Query = tag.Trim(), Page = page ?? 1, PageSize = 20 };

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(
                result.Documents.OrderByDescending(p => p.Score).Select(p => p.DocumentId).ToList(), searchTerm.Page.Value,
                searchTerm.PageSize.Value);

            var viewModel = new SearchResultModel
            {
                TimeElapsed = result.ElapsedMilliseconds,
                SearchTerm = searchTerm,
                NumberOfItemsFound = posts.Count,
                SearchResult = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url),
                ShowAdvancedSearchPanel = false,
                CardViewStyles = ViewStyles.Small,

            };

            return View("Index", viewModel);
        }

        [Route("{lang}/Keywords/{keyword}", Order = 0)]
        [Route("Keywords/{keyword}", Order = 1)]
        public virtual async Task<ActionResult> Keyword(string keyword, int? page)
        {
            if (keyword.IsNullOrWhiteSpace())
            {
                return View("Index");
            }

            var result = await _searchEngine.SearchAsync($"\"{keyword}\"", null, LanguagesService.GetDefaultLanguage().Id, null, SearchPlace.Keywords);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return View("Error");
            }

            var searchTerm = new SearchTermModel { Query = keyword.Trim(), Page = page ?? 1, PageSize = 20 };

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(
                result.Documents.OrderByDescending(p => p.Score).Select(p => p.DocumentId).ToList(), searchTerm.Page.Value,
                searchTerm.PageSize.Value);

            var viewModel = new SearchResultModel
            {
                TimeElapsed = result.ElapsedMilliseconds,
                SearchTerm = searchTerm,
                NumberOfItemsFound = posts.Count,
                SearchResult = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url),
                ShowAdvancedSearchPanel = false,
                CardViewStyles = ViewStyles.Small,

            };
            
            return View("Index", viewModel);
        }

        public virtual ActionResult MoreLikeThis(int postId, PostType postType, int? numberOfSimilarityPosts)
        {
            var result = _searchEngine.MoreLikeThis(postId, null, 0, postType, SearchPlace.Anywhere, numberOfSimilarityPosts ?? 5);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return Content("");
            }

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(
                result.Documents.OrderByDescending(p => p.Score).Select(p => p.DocumentId).ToList(), 1,
                numberOfSimilarityPosts ?? 5);
            return PartialView("Partials/_MoreLikeThis", _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url));
        }

        public virtual async Task<JsonResult> SearchSuggestion(string searchString)
        {
            var result = await _searchEngine.AutoCompleteAsync(searchString);
            if (result.HasError)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(result.Error, System.Web.HttpContext.Current));
                ViewBag.ErrorCode = errorCode;
                return Json("", JsonRequestBehavior.AllowGet);
            }

            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            var posts = _postService.GetItemsById(result.Documents.OrderByDescending(p => p.Score).Select(p => p.DocumentId).ToList());
            var model = _postModelFactory.PreparePostCardViewModel(posts, currentUser, Url);

            return Json(new {suggestions = model.Select(p => new {value = p.Title, data = p.PostUrl}).ToList()},
                JsonRequestBehavior.AllowGet);
        }
    }
}