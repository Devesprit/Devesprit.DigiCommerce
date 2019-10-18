using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.DigiCommerce.Models.Products;
using Devesprit.DigiCommerce.Models.Search;
using Devesprit.Services.Blog;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Posts;
using Devesprit.Services.Products;
using Devesprit.Services.SearchEngine;
using Devesprit.WebFramework.ActionResults;
using X.PagedList;

namespace Devesprit.DigiCommerce.Controllers
{
    [Intercept(typeof(MethodCache))]
    public partial class RssFeedController : BaseController
    {
        private readonly IPostService<TblPosts> _postService;
        private readonly IProductService _productService;
        private readonly IBlogPostService _blogPostService;

        public RssFeedController(IPostService<TblPosts> postService,
            IProductService productService,
            IBlogPostService blogPostService)
        {
            _postService = postService;
            _productService = productService;
            _blogPostService = blogPostService;
        }

        [Route("{lang}/RSSFeed/{listType}", Order = 0)]
        [Route("RSSFeed/{listType}", Order = 1)]
        [MethodCache(Tags = new[] { nameof(TblBlogPosts), nameof(TblProducts) }, VaryByCustom = "lang")]
        public virtual ActionResult Index(PostsListType listType, int? page, int? pageSize, int? catId, DateTime? fromDate)
        {
            IPagedList<TblPosts> posts = null;
            switch (listType)
            {
                case PostsListType.Newest:
                    posts = _postService.GetNewItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case PostsListType.MostPopular:
                    posts = _postService.GetPopularItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case PostsListType.HotList:
                    posts = _postService.GetHotList(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case PostsListType.Featured:
                    posts = _postService.GetFeaturedItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
            }

            var feed = new SyndicationFeed(CurrentSettings.GetLocalized(x=> x.SiteName), CurrentSettings.GetLocalized(x => x.SiteDescription),
                new Uri(Url.Action("Index", "Home", null, Request.Url.Scheme)));

            List<SyndicationItem> items = new List<SyndicationItem>();
            foreach (var post in posts)
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
                    url = new Uri(Url.Action("Post", "Blog", new {slug = post.Slug}, Request.Url.Scheme));
                }
                if (post.PostType == PostType.Product)
                {
                    url = new Uri(Url.Action("Index", "Product", new {slug = post.Slug}, Request.Url.Scheme));
                }

                var item = new SyndicationItem(
                    post.GetLocalized(p => p.Title).EscapeXml(), (post.Descriptions?.OrderBy(p => p.DisplayOrder)
                                                         ?.FirstOrDefault()
                                                         ?.GetLocalized(p => p.HtmlDescription) ?? " - ").EscapeXml(),
                    url,
                    post.Id.ToString(), post.LastUpDate ?? post.PublishDate);
                items.Add(item);
            }

            feed.Items = items;
            return new RssActionResult(new Rss20FeedFormatter(feed));
        }

        [Route("{lang}/Products/RSS/{listType}", Order = 0)]
        [Route("Products/RSS/{listType}", Order = 1)]
        [MethodCache(Tags = new[] { nameof(TblBlogPosts), nameof(TblProducts) }, VaryByCustom = "lang")]
        public virtual ActionResult Products(ProductsListType listType, int? page, int? pageSize, int? catId, DateTime? fromDate)
        {
            IPagedList<TblProducts> products = null;
            switch (listType)
            {
                case ProductsListType.Newest:
                    products = _productService.GetNewItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case ProductsListType.MostPopular:
                    products = _productService.GetPopularItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case ProductsListType.HotList:
                    products = _productService.GetHotList(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case ProductsListType.Featured:
                    products = _productService.GetFeaturedItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case ProductsListType.BestSelling:
                    products = _productService.GetBestSelling(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case ProductsListType.MostDownloaded:
                    products = _productService.GetMostDownloadedItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
            }

            var feed = new SyndicationFeed(CurrentSettings.GetLocalized(x=> x.SiteName), CurrentSettings.GetLocalized(x => x.SiteDescription),
                new Uri(Url.Action("Index", "Home", null, Request.Url.Scheme)));

            var items = Enumerable.Select(products, product => new SyndicationItem(
                    product.GetLocalized(p => p.Title).EscapeXml(), (product.Descriptions?.OrderBy(p => p.DisplayOrder)
                                                                   ?.FirstOrDefault()
                                                                   ?.GetLocalized(p => p.HtmlDescription) ?? " - ").EscapeXml(),
                    new Uri(Url.Action("Index", "Product", new {slug = product.Slug}, Request.Url.Scheme)),
                    product.Id.ToString(), product.LastUpDate ?? product.PublishDate))
                .ToList();
            feed.Items = items;
            return new RssActionResult(new Rss20FeedFormatter(feed));
        }

        [Route("{lang}/Blog/RSS/{listType}", Order = 0)]
        [Route("Blog/RSS/{listType}", Order = 1)]
        [MethodCache(Tags = new[] { nameof(TblBlogPosts), nameof(TblProducts) }, VaryByCustom = "lang")]
        public virtual ActionResult Blog(PostsListType listType, int? page, int? pageSize, int? catId, DateTime? fromDate)
        {
            IPagedList<TblBlogPosts> posts = null;
            switch (listType)
            {
                case PostsListType.Newest:
                    posts = _blogPostService.GetNewItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case PostsListType.MostPopular:
                    posts = _blogPostService.GetPopularItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case PostsListType.HotList:
                    posts = _blogPostService.GetHotList(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
                case PostsListType.Featured:
                    posts = _blogPostService.GetFeaturedItems(page ?? 1, pageSize ?? 50, catId, fromDate);
                    break;
            }

            var feed = new SyndicationFeed(CurrentSettings.GetLocalized(x=> x.SiteName), CurrentSettings.GetLocalized(x => x.SiteDescription),
                new Uri(Url.Action("Index", "Home", null, Request.Url.Scheme)));

            var items = Enumerable.Select(posts, post => new SyndicationItem(
                    post.GetLocalized(p => p.Title).EscapeXml(), (post.Descriptions?.OrderBy(p => p.DisplayOrder)
                                                                   ?.FirstOrDefault()
                                                                   ?.GetLocalized(p => p.HtmlDescription) ?? " - ").EscapeXml(),
                    new Uri(Url.Action("Post", "Blog", new {slug = post.Slug}, Request.Url.Scheme)),
                    post.Id.ToString(), post.LastUpDate ?? post.PublishDate))
                .ToList();
            feed.Items = items;
            return new RssActionResult(new Rss20FeedFormatter(feed));
        }
    }
}