using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services;
using Devesprit.Services.Languages;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Pages;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.SEO;
using Devesprit.Services.Users;
using Devesprit.Utilities.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Devesprit.DigiCommerce.Controllers
{
    [Intercept(typeof(MethodCache))]
    public partial class SiteMapController : Controller
    {
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly IPostService<TblPosts> _postService;
        private readonly IPostCategoriesService _categoriesService;
        private readonly IPostTagsService _tagsService;
        private readonly IPagesService _pagesService;
        private readonly ILanguagesService _languagesService;
        private readonly ISettingService _settingService;

        public SiteMapController(ISitemapGenerator sitemapGenerator, 
            IPostService<TblPosts> postService,
            IPostCategoriesService categoriesService,
            IPostTagsService tagsService,
            IPagesService pagesService,
            ILanguagesService languagesService,
            ISettingService settingService)
        {
            _sitemapGenerator = sitemapGenerator;
            _postService = postService;
            _categoriesService = categoriesService;
            _tagsService = tagsService;
            _pagesService = pagesService;
            _languagesService = languagesService;
            _settingService = settingService;
        }

        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().Get<ApplicationUserManager>();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Response != null)
            {
                Response.AddHeader("Set-Cookie", "HttpOnly;Secure;SameSite=Lax");
            }

            if (!filterContext.IsChildAction)
            {
                //Save user latest IP address
                if (User.Identity.IsAuthenticated)
                {
                    DependencyResolver.Current.GetService<IUsersService>()
                        .SetUserLatestIpAndLoginDate(User.Identity.GetUserId(), HttpContext.GetClientIpAddress());
                }

                //If user disabled
                var accountDisabledUrl = Url.Action("AccountDisabled", "User");
                if (User.Identity.IsAuthenticated && filterContext.HttpContext?.Request.Url?.LocalPath != accountDisabledUrl)
                {
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    if (user != null && user.UserDisabled)
                    {
                        filterContext.Result = new RedirectResult(accountDisabledUrl, true);
                    }
                }

                //Append 'https' & 'www' to Url and redirect it
                if (Request.Url != null && Request.HttpMethod.ToLower() == "get")
                {
                    var siteUrl = _settingService.LoadSetting<SiteSettings>().SiteUrl;
                    Uri siteUri = Request.Url;
                    if (siteUrl.IsValidUrl())
                    {
                        siteUri = new Uri(siteUrl);
                    }

                    var normalizedPathAndQuery = Request.Url.PathAndQuery;
                    var port = "";
                    if (Request.Url.Port != 443 && Request.Url.Port != 80 && Request.Url.Port != 0)
                    {
                        port = ":" + Request.Url.Port;
                    }

                    if ($"{Request.Url.Scheme}://{Request.Url.Host}".ToLower().Trim().TrimEnd('/') != $"{siteUri.Scheme}://{siteUri.Host}".ToLower().Trim().TrimEnd('/'))
                    {
                        if (!Response.IsRequestBeingRedirected)
                        {

                            filterContext.Result = new RedirectResult($"{siteUri.Scheme}://{siteUri.Host}{port}{normalizedPathAndQuery}", false);
                            return;
                        }
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        // GET: SiteMap
        [MethodCache(Tags = new[] { nameof(TblBlogPosts), nameof(TblProducts) })]
        public virtual async Task<ActionResult> Index()
        {
            var items = new List<SitemapItem>();
            var languagesList = _languagesService.GetAsEnumerable().Where(p => p.Published)
                .OrderByDescending(p => p.IsDefault);
            
            foreach (var language in languagesList)
            {
                items.Add(new SitemapItem(
                    Url.Action("Index", "Home",
                        new {lang = language.IsoCode},
                        Request.Url.Scheme), DateTime.Now,
                    SitemapChangeFrequency.Daily, 1));

                items.AddRange((await _pagesService.GetAsEnumerableAsync()).Where(p=> p.Published).Select(page =>
                    new SitemapItem(
                        Url.Action("Index", "Page", new { slug = page.Slug, lang = language.IsoCode },
                            Request.Url.Scheme), DateTime.Now,
                        SitemapChangeFrequency.Daily, 1)));

                items.AddRange((await _tagsService.GetAsEnumerableAsync()).Select(tag =>
                    new SitemapItem(
                        Url.Action("Tag", "Search",
                            new { tag = tag.GetLocalized(x => x.Tag, language.Id), lang = language.IsoCode },
                            Request.Url.Scheme), DateTime.Now,
                        SitemapChangeFrequency.Daily, 0.9)).DistinctBy(p=> p.Url));


                items.AddRange((await _categoriesService.GetAsEnumerableAsync()).Select(category =>
                    new SitemapItem(
                        Url.Action("FilterByCategory", "Product", new { slug = category.Slug, lang = language.IsoCode },
                            Request.Url.Scheme), DateTime.Now,
                        SitemapChangeFrequency.Daily, 0.8)));

                items.AddRange((await _categoriesService.GetAsEnumerableAsync()).Select(category =>
                    new SitemapItem(
                        Url.Action("FilterByCategory", "Blog", new { slug = category.Slug, lang = language.IsoCode },
                            Request.Url.Scheme), DateTime.Now,
                        SitemapChangeFrequency.Daily, 0.8)));


                foreach (var post in _postService.GetNewItemsForSiteMap())
                {
                    Uri url = new Uri(Url.Action("Index", "Search", new
                    {
                        lang = language.IsoCode,
                        OrderBy = SearchResultSortType.Score,
                        SearchPlace = SearchPlace.Title,
                        Query = post.Title
                    }, Request.Url.Scheme));

                    if (post.PostType == PostType.BlogPost)
                    {
                        url = new Uri(Url.Action("Post", "Blog", new { slug = post.Slug, lang = language.IsoCode }, Request.Url.Scheme));
                    }
                    if (post.PostType == PostType.Product)
                    {
                        url = new Uri(Url.Action("Index", "Product", new { slug = post.Slug, lang = language.IsoCode }, Request.Url.Scheme));
                    }

                    items.Add(new SitemapItem(url.ToString(), post.LastUpDate ?? post.PublishDate,
                        SitemapChangeFrequency.Weekly, 0.7));
                }
            }

            return Content(_sitemapGenerator.GenerateSiteMap(items).ToString(), "text/xml", Encoding.UTF8);
        }
    }
}