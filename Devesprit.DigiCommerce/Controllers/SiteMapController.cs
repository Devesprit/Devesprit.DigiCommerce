﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
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
                        filterContext.Result = new RedirectResult(accountDisabledUrl, false);
                    }
                }

                //Append 'https' & 'www' to Url and redirect it
                if (Request.Url != null && Request.HttpMethod.ToLower() == "get")
                {
                    var currentSettings = _settingService.LoadSetting<SiteSettings>();
                    Uri siteUri = Request.Url;
                    if (currentSettings.SiteUrl.IsValidUrl() && currentSettings.RedirectAllRequestsToSiteUrl)
                    {
                        siteUri = new Uri(currentSettings.SiteUrl);
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

                            filterContext.Result = new RedirectResult($"{siteUri.Scheme}://{siteUri.Host}{port}{normalizedPathAndQuery}", true);
                            return;
                        }
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private string GenerateUrl(string action, string controller, object routeValues)
        {
            return Url.Action(action, controller, routeValues, Request.Url.Scheme);
        }

        private List<Tuple<string, string>> GenerateAlternateUrls(List<TblLanguages> languagesList, string action, string controller, object routeValues, SiteSettings settings)
        {
            if (languagesList.Count == 1)
            {
                return null;
            }
            
            var result = new List<Tuple<string, string>>();
            var uri = new Uri(Url.Action(action, controller, new RouteValueDictionary(routeValues), Request.Url.Scheme));
            var allLangsIso = languagesList.Select(p => p.IsoCode).ToList();
            if(settings.AppendLanguageCodeToUrl)
            {
                result.Add(new Tuple<string, string>(
                        "x-default",
                        uri.SetLangIso(languagesList.FirstOrDefault(p=> p.IsDefault).IsoCode, allLangsIso).ToString()
                    ));
            }
            else
            {
                result.Add(new Tuple<string, string>(
                        "x-default",
                        uri.RemoveLangIso(allLangsIso).ToString()
                    ));
            }
            foreach (var lang in languagesList)
            {
                result.Add(new Tuple<string, string>(
                    lang.IsoCode,
                    uri.SetLangIso(lang.IsoCode, allLangsIso).ToString()
                ));
            }

            return result;
        }

        private List<Tuple<string, string>> GenerateAlternateUrlsForTags(List<TblLanguages> languagesList, TblPostTags tag, SiteSettings settings)
        {
            if (languagesList.Count == 1)
            {
                return null;
            }

            var action = "Tag";
            var controller = "Search";
            var result = new List<Tuple<string, string>>();

            var allLangsIso = languagesList.Select(p => p.IsoCode).ToList();
            var defaultLang = languagesList.FirstOrDefault(p => p.IsDefault);
            if (settings.AppendLanguageCodeToUrl)
            {
                var routeValueDictionary = new RouteValueDictionary
                {
                    {"tag", tag.GetLocalized(x => x.Tag, defaultLang.Id).UrlEncode()},
                    {"page", 1 }
                };
                var uri = new Uri(Url.Action(action, controller, routeValueDictionary, Request.Url.Scheme));

                result.Add(new Tuple<string, string>(
                    "x-default",
                    uri.SetLangIso(defaultLang.IsoCode, allLangsIso).ToString()
                ));
            }
            else
            {
                var routeValueDictionary = new RouteValueDictionary
                {
                    {"tag", tag.GetLocalized(x => x.Tag, defaultLang.Id).UrlEncode()},
                    {"page", 1 }
                };
                var uri = new Uri(Url.Action(action, controller, routeValueDictionary, Request.Url.Scheme));

                result.Add(new Tuple<string, string>(
                    "x-default",
                    uri.RemoveLangIso(allLangsIso).ToString()
                ));
            }
            foreach (var lang in languagesList)
            {
                var routeValueDictionary = new RouteValueDictionary
                {
                    {"tag", tag.GetLocalized(x => x.Tag, lang.Id).UrlEncode()},
                    {"page", 1 }
                };
                var uri = new Uri(Url.Action(action, controller, routeValueDictionary, Request.Url.Scheme));

                result.Add(new Tuple<string, string>(
                    lang.IsoCode,
                    uri.SetLangIso(lang.IsoCode, allLangsIso).ToString()
                ));
            }

            return result;
        }

        // GET: SiteMap
        [MethodCache(Tags = new[] { nameof(TblBlogPosts), nameof(TblProducts) })]
        [Route("sitemap", Order = 0)]
        [Route("sitemap.xml", Order = 1)]
        public virtual async Task<ActionResult> Index()
        {
            var settings = _settingService.LoadSetting<SiteSettings>();

            var items = new List<SitemapItem>();
            var languagesList = _languagesService.GetAsEnumerable().Where(p => p.Published)
                .OrderBy(p => p.DisplayOrder).ToList();

            var defaultLanguage = await _languagesService.GetDefaultLanguageAsync();
            foreach (var lang in languagesList)
            {
                items.Add(new SitemapItem(
                    GenerateUrl("Index", "Home", new { lang = lang.IsoCode }),
                    DateTime.Now,
                    SitemapChangeFrequency.Daily,
                    1,
                    GenerateAlternateUrls(languagesList, "Index", "Home", null, settings)
                ));

                items.AddRange((await _pagesService.GetAsEnumerableAsync()).Where(p => p.Published).Select(page =>
                    new SitemapItem(
                        GenerateUrl("Index", "Page", new { slug = page.Slug, lang = lang.IsoCode }),
                        DateTime.Now,
                        SitemapChangeFrequency.Daily,
                        0.7,
                        GenerateAlternateUrls(languagesList, "Index", "Page", new { slug = page.Slug }, settings)
                    )));

                items.AddRange((await _tagsService.GetAsEnumerableAsync()).Select(tag =>
                    new SitemapItem(
                        GenerateUrl("Tag", "Search",
                            new { tag = tag.GetLocalized(p=> p.Tag, lang.Id), page = 1, lang = lang.IsoCode }),
                        DateTime.Now,
                        SitemapChangeFrequency.Daily,
                        0.8,
                        GenerateAlternateUrlsForTags(languagesList, tag, settings)
                    )));

                items.AddRange((await _categoriesService.GetAsEnumerableAsync())
                    .Where(p => p.DisplayArea == DisplayArea.Both ||
                                p.DisplayArea == DisplayArea.ProductsSection)
                    .Select(category =>
                        new SitemapItem(
                            GenerateUrl("FilterByCategory", "Product", new { slug = category.Slug, page = 1, lang = lang.IsoCode }),
                            DateTime.Now,
                            SitemapChangeFrequency.Daily,
                            0.9,
                            GenerateAlternateUrls(languagesList, "FilterByCategory", "Product", new { slug = category.Slug, page = 1 }, settings)
                        )));

                if (settings.EnableBlog)
                {
                    items.AddRange((await _categoriesService.GetAsEnumerableAsync())
                        .Where(p => p.DisplayArea == DisplayArea.Both || p.DisplayArea == DisplayArea.BlogSection)
                        .Select(category =>
                            new SitemapItem(
                                GenerateUrl("FilterByCategory", "Blog",
                                    new { slug = category.Slug, page = 1, lang = lang.IsoCode }),
                                DateTime.Now,
                                SitemapChangeFrequency.Daily,
                                0.8,
                                GenerateAlternateUrls(languagesList, "FilterByCategory", "Blog", new { slug = category.Slug, page = 1 }, settings)
                            )));
                }

                foreach (var post in _postService.GetNewItemsForSiteMap())
                {
                    if (post.PostType == PostType.BlogPost && settings.EnableBlog)
                    {
                        items.Add(new SitemapItem(
                            GenerateUrl("Post", "Blog", new { id = post.Id, slug = post.Slug, lang = lang.IsoCode }),
                            post.LastUpDate ?? post.PublishDate,
                            SitemapChangeFrequency.Weekly,
                            0.9,
                            GenerateAlternateUrls(languagesList, "Post", "Blog", new { id = post.Id, slug = post.Slug }, settings)
                        ));
                    }
                    else if (post.PostType == PostType.Product)
                    {
                        items.Add(new SitemapItem(
                            GenerateUrl("Index", "Product", new { id = post.Id, slug = post.Slug, lang = lang.IsoCode }),
                            post.LastUpDate ?? post.PublishDate,
                            SitemapChangeFrequency.Weekly,
                            1,
                            GenerateAlternateUrls(languagesList, "Index", "Product", new { id = post.Id, slug = post.Slug }, settings)
                        ));
                    }
                    else
                    {
                        items.Add(new SitemapItem(
                            GenerateUrl("Index", "Search", new
                            {
                                lang = lang.IsoCode,
                                OrderBy = SearchResultSortType.Score,
                                SearchPlace = SearchPlace.Title,
                                Query = post.Title,
                                Page = 1
                            }),
                            post.LastUpDate ?? post.PublishDate,
                            SitemapChangeFrequency.Weekly,
                            0.9,
                            GenerateAlternateUrls(languagesList, "Index", "Search", new
                            {
                                OrderBy = SearchResultSortType.Score,
                                SearchPlace = SearchPlace.Title,
                                Query = post.Title,
                                Page = 1
                            }, settings)
                        ));
                    }
                }
            }

            return Content(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                _sitemapGenerator.GenerateSiteMap(items).ToString(), "text/xml", Encoding.UTF8);
        }
    }
}