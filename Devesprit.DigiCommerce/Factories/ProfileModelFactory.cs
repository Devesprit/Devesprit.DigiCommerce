using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Profile;
using Devesprit.DigiCommerce.Models.Search;
using Devesprit.Services.Localization;
using Devesprit.Services.Notifications;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.Users;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories
{
    public partial class ProfileModelFactory : IProfileModelFactory
    {
        private readonly IUserGroupsService _userGroupsService;
        private readonly ILocalizationService _localizationService;
        private readonly IPostService<TblPosts> _postService;
        private readonly HttpContextBase _httpContext;
        private readonly INotificationsService _notificationsService;

        public ProfileModelFactory(IUserGroupsService userGroupsService, 
            ILocalizationService localizationService,
            IPostService<TblPosts> postService,
            HttpContextBase httpContext,
            INotificationsService notificationsService)
        {
            _userGroupsService = userGroupsService;
            _localizationService = localizationService;
            _postService = postService;
            _httpContext = httpContext;
            _notificationsService = notificationsService;
        }

        public virtual async Task<ProfileModel> PrepareProfileModelAsync(TblUsers user)
        {
            var result = new ProfileModel
            {
                UserInfo = await PrepareUserInfoModelAsync(user),
                UserUnreadedNotifications = _notificationsService.GetUserUnReadedNotificationsCount(user.Id)
            };

            return result;
        }

        public virtual async Task<UserInfoModel> PrepareUserInfoModelAsync(TblUsers user)
        {
            var result = AutoMapper.Mapper.Map<UserInfoModel>(user);
            result.ShowUserSubscriptionInfo = (await _userGroupsService.GetAsEnumerableAsync()).Any();

            if (user.UserCountry != null)
            {
                result.Country = user.UserCountry.GetLocalized(x=> x.CountryName);
            }

            if (user.UserGroup != null && user.SubscriptionExpireDate > DateTime.Now)
            {
                var textColor = string.IsNullOrWhiteSpace(user.UserGroup.GetLocalized(x=> x.GroupTextColor)) ? "inherit" : user.UserGroup.GetLocalized(x => x.GroupTextColor);
                var backgroundColor = string.IsNullOrWhiteSpace(user.UserGroup.GetLocalized(x => x.GroupBackgroundColor)) ? "inherit" : user.UserGroup.GetLocalized(x=> x.GroupBackgroundColor);
                var groupImage = string.IsNullOrWhiteSpace(user.UserGroup.GetLocalized(x => x.GroupSmallIcon))
                    ? ""
                    : $@"<img style=""max-height: 18px;"" src=""{
                            user.UserGroup.GetLocalized(x => x.GroupSmallIcon)
                        }""/>";

                result.UserGroup =
                    $@"<span class=""badge"" style=""color: {textColor}; background-color:{backgroundColor}"">{
                            groupImage
                        } {user.UserGroup.GetLocalized(x => x.GroupName)}</span>";

                var highestGroup = await _userGroupsService.GetHighestUserGroupAsync();
                result.UserSubscribedToHighestPlan = user.UserGroup.GroupPriority >= highestGroup.GroupPriority;

                if (user.UserGroup.MaxDownloadCount > 0)
                {
                    var maxDownloadPeriodTypeStr = _localizationService.GetResource(user.UserGroup.MaxDownloadPeriodType.ToString());
                    result.UserGroupDownloadLimit = user.UserGroup.MaxDownloadCount + _localizationService.GetResource("Per") + maxDownloadPeriodTypeStr;
                }
                else
                {
                    result.UserGroupDownloadLimit = _localizationService.GetResource("Unlimited");
                }
            }
            else
            {
                result.UserGroup = $"<small>({_localizationService.GetResource("YouNotSubscribedToPlan")})</small>";
                result.SubscriptionDate = null;
                result.SubscriptionExpireDate = null;
            }

            if (user.MaxDownloadCount > 0)
            {
                var maxDownloadPeriodTypeStr = _localizationService.GetResource(user.MaxDownloadPeriodType.ToString());
                result.DownloadLimit = user.MaxDownloadCount + _localizationService.GetResource("Per") + maxDownloadPeriodTypeStr;
            }
            else
            {
                result.DownloadLimit = _localizationService.GetResource("Unlimited");
            }

            return result;
        }

        public virtual UpdateProfileModel PrepareUpdateProfileModel(TblUsers user)
        {
            return AutoMapper.Mapper.Map<UpdateProfileModel>(user);
        }

        public virtual async Task<IPagedList<UserLikeWishlistModel>> PrepareUserLikedEntitiesModelAsync(IPagedList<TblUserLikes> likes)
        {
            var urlHelper = new UrlHelper(_httpContext.Request.RequestContext);
            var result = new List<UserLikeWishlistModel>();
            foreach (var item in likes)
            {
                var userLikeWishlistModel = Mapper.Map<UserLikeWishlistModel>(item);
                var post = await _postService.FindByIdAsync(item.PostId);
                userLikeWishlistModel.PostTitle = post.GetLocalized(p => p.Title);

                Uri url = new Uri(urlHelper.Action("Index", "Search", new SearchTermModel()
                {
                    PostType = null,
                    OrderBy = SearchResultSortType.Score,
                    SearchPlace = SearchPlace.Title,
                    Query = post.Title
                }, _httpContext.Request.Url.Scheme));

                if (post.PostType == PostType.BlogPost)
                {
                    url = new Uri(urlHelper.Action("Post", "Blog", new { slug = post.Slug }, _httpContext.Request.Url.Scheme));
                }
                if (post.PostType == PostType.Product)
                {
                    url = new Uri(urlHelper.Action("Index", "Product", new { slug = post.Slug }, _httpContext.Request.Url.Scheme));
                }

                userLikeWishlistModel.PostHomePageUrl = url.ToString();
                
                result.Add(userLikeWishlistModel);
            }
            return new StaticPagedList<UserLikeWishlistModel>(
                result,
                likes.PageNumber,
                likes.PageSize,
                likes.TotalItemCount);
        }

        public virtual async Task<IPagedList<UserLikeWishlistModel>> PrepareUserWishlistModelAsync(IPagedList<TblUserWishlist> wishlist)
        {
            var urlHelper = new UrlHelper(_httpContext.Request.RequestContext);
            var result = new List<UserLikeWishlistModel>();
            foreach (var item in wishlist)
            {
                var userLikeWishlistModel = Mapper.Map<UserLikeWishlistModel>(item);
                var post = await _postService.FindByIdAsync(item.PostId);
                userLikeWishlistModel.PostTitle = post.GetLocalized(p => p.Title);

                Uri url = new Uri(urlHelper.Action("Index", "Search", new SearchTermModel()
                {
                    PostType = null,
                    OrderBy = SearchResultSortType.Score,
                    SearchPlace = SearchPlace.Title,
                    Query = post.Title
                }, _httpContext.Request.Url.Scheme));

                if (post.PostType == PostType.BlogPost)
                {
                    url = new Uri(urlHelper.Action("Post", "Blog", new { slug = post.Slug }, _httpContext.Request.Url.Scheme));
                }
                if (post.PostType == PostType.Product)
                {
                    url = new Uri(urlHelper.Action("Index", "Product", new { slug = post.Slug }, _httpContext.Request.Url.Scheme));
                }

                userLikeWishlistModel.PostHomePageUrl = url.ToString();

                result.Add(userLikeWishlistModel);
            }
            return new StaticPagedList<UserLikeWishlistModel>(
                result,
                wishlist.PageNumber,
                wishlist.PageSize,
                wishlist.TotalItemCount);
        }
    }
}