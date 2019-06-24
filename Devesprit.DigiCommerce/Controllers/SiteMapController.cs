using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Models.Search;
using Devesprit.Services.Languages;
using Devesprit.Services.Localization;
using Devesprit.Services.Pages;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.SEO;
using Devesprit.Utilities.Extensions;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class SiteMapController : Controller
    {
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly IPostService<TblPosts> _postService;
        private readonly IPostCategoriesService _categoriesService;
        private readonly IPostTagsService _tagsService;
        private readonly IPagesService _pagesService;
        private readonly ILanguagesService _languagesService;

        public SiteMapController(ISitemapGenerator sitemapGenerator, 
            IPostService<TblPosts> postService,
            IPostCategoriesService categoriesService,
            IPostTagsService tagsService,
            IPagesService pagesService,
            ILanguagesService languagesService)
        {
            _sitemapGenerator = sitemapGenerator;
            _postService = postService;
            _categoriesService = categoriesService;
            _tagsService = tagsService;
            _pagesService = pagesService;
            _languagesService = languagesService;
        }

        // GET: SiteMap
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


                foreach (var post in _postService.GetNewItems())
                {
                    Uri url = new Uri(Url.Action("Index", "Search", new SearchTermModel()
                    {
                        PostType = null,
                        OrderBy = SearchResultSortType.Score,
                        SearchPlace = SearchPlace.Title,
                        Query = post.Title
                    }, Request.Url.Scheme));

                    if (post.PostType == PostType.BlogPost)
                    {
                        url = new Uri(Url.Action("Post", "Blog", new { slug = post.Slug }, Request.Url.Scheme));
                    }
                    if (post.PostType == PostType.Product)
                    {
                        url = new Uri(Url.Action("Index", "Product", new { slug = post.Slug }, Request.Url.Scheme));
                    }

                    items.Add(new SitemapItem(url.ToString(), post.LastUpDate ?? post.PublishDate,
                        SitemapChangeFrequency.Weekly, 0.7));
                }
            }

            return Content(_sitemapGenerator.GenerateSiteMap(items).ToString(), "text/xml", Encoding.UTF8);
        }
    }
}