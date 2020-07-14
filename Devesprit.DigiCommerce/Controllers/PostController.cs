using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.Services.Posts;
using Devesprit.Services.Users;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class PostController : BaseController
    {
        private readonly IPostService<TblPosts> _postService;
        private readonly IUserLikesService _userLikesService;
        private readonly IUserWishlistService _userWishlistService;

        public PostController(
            IPostService<TblPosts> postService,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService)
        {
            _postService = postService;
            _userLikesService = userLikesService;
            _userWishlistService = userWishlistService;
        } 

        [HttpPost]
        public virtual async Task<ActionResult> LikePost(int postId)
        {
            var userId = User.Identity.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { response = "mustLogin" });
            }
            var post = await _postService.FindByIdAsync(postId);
            var result = await _userLikesService.LikePostAsync(postId, userId, post.PostType);
            if (result)
            {
                await _postService.IncreaseNumberOfLikesAsync(post);
                return Json(new { response = "add"});
            }
            await _postService.IncreaseNumberOfLikesAsync(post, -1);
            return Json(new { response = "remove" });
        }

        [HttpPost]
        public virtual async Task<ActionResult> AddPostToWishlist(int postId)
        {
            var userId = User.Identity.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { response = "mustLogin" });
            }

            var post = await _postService.FindByIdAsync(postId);
            var result = await _userWishlistService.AddPostToUserWishlistAsync(postId, userId, post.PostType);
            return Json(new { response = result ? "add" : "remove" });
        }

        [ChildActionOnly]
        public virtual ActionResult LikeWishlistButtonsPartialView(LikeWishlistButtonsModel model)
        {
            return View("Partials/_LikeWishlistButtons", model); 
        }

        public virtual ActionResult PostCardViewPartialView(PostCardViewModel model, ViewStyles style = ViewStyles.Large)
        {
            switch (style)
            {
                case ViewStyles.Large:
                    return View("Partials/_PostLargeCardView", model);
                case ViewStyles.Normal:
                    return View("Partials/_PostNormalCardView", model);
                case ViewStyles.Small:
                    return View("Partials/_PostSmallCardView", model);
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }
    }
}