using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.DigiCommerce.Models.Search;
using Devesprit.Services.Localization;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.Users;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories
{
    public partial class PostModelFactory : IPostModelFactory
    {
        private readonly IPostService<TblPosts> _postService;
        private readonly IUserLikesService _userLikesService;
        private readonly IUserWishlistService _userWishlistService;
        private readonly HttpContextBase _httpContext;

        public PostModelFactory(IPostService<TblPosts> postService,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService,
            HttpContextBase httpContext)
        {
            _postService = postService;
            _userLikesService = userLikesService;
            _userWishlistService = userWishlistService;
            _httpContext = httpContext;
        }

        public virtual PostCardViewModel PreparePostCardViewModel(TblPosts post, TblUsers currentUser,
            UrlHelper url)
        {
            var result = AutoMapper.Mapper.Map<PostCardViewModel>(post);
            var likesCount = _postService.GetNumberOfLikes(post.Id);
            result.NumberOfLikes = likesCount;
            result.LastUpDate = post.LastUpDate ?? post.PublishDate;
            result.MainImageUrl = post.Images?.OrderBy(p => p.DisplayOrder).FirstOrDefault()
                                         ?.ImageUrl ?? "";
            result.Categories = post.Categories
                .Select(p => new PostCategoriesModel()
                {
                    Id = p.Id,
                    CategoryName = p.GetLocalized(x => x.CategoryName),
                    Slug = p.Slug,
                    CategoryUrl = post.PostType == PostType.BlogPost ? 
                        url.Action("FilterByCategory", "Blog", new { slug = p.Slug }) :
                        url.Action("FilterByCategory", "Product", new { slug = p.Slug })
                })
                .ToList();
            var desc = post.Descriptions?.OrderBy(p => p.DisplayOrder).FirstOrDefault()?.GetLocalized(x => x.HtmlDescription) ?? "";
            result.DescriptionTruncated = desc.ConvertHtmlToText().TruncateText(350);

            result.LikeWishlistButtonsModel = new LikeWishlistButtonsModel()
            {
                PostId = post.Id,
                AlreadyAddedToWishlist = _userWishlistService.UserAddedThisPostToWishlist(post.Id, currentUser?.Id),
                AlreadyLiked = _userLikesService.UserLikedThisPost(post.Id, currentUser?.Id)
            };

            if (post.PostType == PostType.BlogPost)
            {
                result.PostUrl = new Uri(url.Action("Post", "Blog", new { slug = post.Slug }, _httpContext.Request.Url.Scheme)).ToString();
            }
            else if (post.PostType == PostType.Product)
            {
                result.PostUrl = new Uri(url.Action("Index", "Product", new { slug = post.Slug }, _httpContext.Request.Url.Scheme)).ToString();
            }
            else
            {
                result.PostUrl = new Uri(url.Action("Index", "Search", new SearchTermModel()
                {
                    PostType = null,
                    OrderBy = SearchResultSortType.Score,
                    SearchPlace = SearchPlace.Title,
                    Query = post.Title
                }, _httpContext.Request.Url.Scheme)).ToString();
            }

            return result;
        }

        public virtual IPagedList<PostCardViewModel> PreparePostCardViewModel(IPagedList<TblPosts> posts,
            TblUsers currentUser, UrlHelper url)
        {
            return new StaticPagedList<PostCardViewModel>(posts.Select(post =>
                    PreparePostCardViewModel(post, currentUser, url)), posts.PageNumber,
                posts.PageSize, posts.TotalItemCount); 
        }

        public virtual PostModel PreparePostModel(TblPosts post, TblUsers currentUser,
            UrlHelper url)
        {
            var result = AutoMapper.Mapper.Map<PostModel>(post);
            result.Title = post.GetLocalized(p => p.Title);
            result.PageTitle = post.GetLocalized(p => p.PageTitle);
            result.MetaDescription = post.GetLocalized(p => p.MetaDescription);
            result.MetaKeyWords = post.GetLocalized(p => p.MetaKeyWords);

            var likesCount = _postService.GetNumberOfLikes(post.Id);
            result.NumberOfLikes = likesCount;
            result.LastUpdate = post.LastUpDate ?? post.PublishDate;
            result.Categories = post.Categories
                .Select(p => new PostCategoriesModel()
                {
                    Id = p.Id,
                    CategoryName = p.GetLocalized(x => x.CategoryName),
                    Slug = p.Slug,
                    CategoryUrl = post.PostType == PostType.BlogPost ?
                        url.Action("FilterByCategory", "Blog", new { slug = p.Slug }) :
                        url.Action("FilterByCategory", "Product", new { slug = p.Slug })
                })
                .ToList();
            result.TagsList = post.Tags
                .Select(p => new Tuple<int, string>(p.Id, p.GetLocalized(x => x.Tag)))
                .ToList();

            result.LikeWishlistButtonsModel = new LikeWishlistButtonsModel()
            {
                PostId = post.Id,
                AlreadyAddedToWishlist = _userWishlistService.UserAddedThisPostToWishlist(post.Id, currentUser?.Id),
                AlreadyLiked = _userLikesService.UserLikedThisPost(post.Id, currentUser?.Id)
            };

            result.Images.Clear();
            foreach (var img in post.Images.OrderBy(p => p.DisplayOrder))
            {
                result.Images.Add(new PostImagesModel()
                {
                    Title = img.GetLocalized(p => p.Title) ?? result.PageTitle,
                    Alt = img.GetLocalized(p => p.Alt) ?? result.Title,
                    ImageUrl = img.GetLocalized(p => p.ImageUrl),
                    DisplayOrder = img.DisplayOrder
                });
            }

            result.Descriptions.Clear();
            foreach (var desc in post.Descriptions.OrderBy(p => p.DisplayOrder))
            {
                var description = desc.GetLocalized(x => x.HtmlDescription);
                result.Descriptions.Add(new PostDescriptionsModel()
                {
                    Description = description,
                    Title = desc.GetLocalized(x => x.Title),
                    IsRtl = description.StripHtml().IsRtlLanguage()
                });
            }

            result.Attributes.Clear();
            foreach (var attr in post.Attributes)
            {
                result.Attributes.Add(new PostAttributesModel()
                {
                    Type = attr.PostAttribute.AttributeType,
                    Name = attr.PostAttribute.GetLocalized(p => p.Name),
                    Value = attr.PostAttribute.AttributeType == PostAttributeType.Option
                        ? attr.AttributeOption.GetLocalized(p => p.Name)
                        : attr.GetLocalized(p => p.Value),
                    DisplayOrder = attr.DisplayOrder
                });
            }

            if (post.PostType == PostType.BlogPost)
            {
                result.PostUrl = new Uri(url.Action("Post", "Blog", new { slug = post.Slug }, _httpContext.Request.Url.Scheme)).ToString();
            }
            else if (post.PostType == PostType.Product)
            {
                result.PostUrl = new Uri(url.Action("Index", "Product", new { slug = post.Slug }, _httpContext.Request.Url.Scheme)).ToString();
            }
            else
            {
                result.PostUrl = new Uri(url.Action("Index", "Search", new SearchTermModel()
                {
                    PostType = null,
                    OrderBy = SearchResultSortType.Score,
                    SearchPlace = SearchPlace.Title,
                    Query = post.Title
                }, _httpContext.Request.Url.Scheme)).ToString();
            }

            return result;
        }
    }
}