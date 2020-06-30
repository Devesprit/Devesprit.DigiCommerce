using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Comment;
using Devesprit.Services.Comments;
using Devesprit.Utilities;
using Microsoft.AspNet.Identity;
using reCaptcha;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class CommentController : BaseController
    {
        private readonly ICommentsService _commentsService;
        private readonly ICommentModelFactory _commentModelFactory;
        private readonly ILocalizationService _localizationService;

        public CommentController(ICommentsService commentsService,
            ICommentModelFactory commentModelFactory,
            ILocalizationService localizationService)
        {
            _commentsService = commentsService;
            _commentModelFactory = commentModelFactory;
            _localizationService = localizationService;
        }

        public virtual ActionResult CommentsList(int postId, int? page)
        {
            var isAdmin = HttpContext.User.IsInRole("Admin");
            var comments = AsyncHelper
                .RunSync(() => _commentsService.GetAsPagedListAsync(!isAdmin, postId, page ?? 1, 15));
            return View(_commentModelFactory.PrepareCommentsListModel(comments, postId, isAdmin));
        }

        public virtual async Task<ActionResult> FindComment(int postId, int commentId)
        {
            var isAdmin = HttpContext.User.IsInRole("Admin");
            var comments = await _commentsService.FindCommentInListAsync(!isAdmin, postId, commentId, 15);
            var model = _commentModelFactory.PrepareCommentsListModel(comments, postId, isAdmin);
            model.HighlightComment = commentId;
            return View("CommentsList", model);
        }


        public virtual ActionResult CommentEditor(int postId)
        {
            var isAdmin = HttpContext.User.IsInRole("Admin");
            var currentUser = UserManager.FindById(User.Identity.GetUserId());

            if (CurrentSettings.UseGoogleRecaptchaForComment)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
            }
            
            return View(_commentModelFactory.PrepareCommentEditorModel(postId, currentUser, isAdmin));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> CommentEditor(CommentEditorModel model)
        {
            var isAdmin = HttpContext.User.IsInRole("Admin");
            var currentUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var currentUserId = currentUser?.Id ?? "";

            if (CurrentSettings.UseGoogleRecaptchaForComment)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
                if (!ReCaptcha.Validate(CurrentSettings.GoogleRecaptchaSecretKey))
                {
                    ViewBag.RecaptchaLastErrors = ReCaptcha.GetLastErrors(HttpContext);
                    return View(model);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var published = CurrentSettings.AutoPublishComments;
            if (isAdmin)
            {
                published = model.Published;
            }

            await _commentsService.AddAsync(
                await _commentModelFactory.PrepareTblCommentsAsync(model, currentUser?.Id, published), true);

            if (published || isAdmin)
            {
                TempData["SuccessNotification"] = _localizationService.GetResource("YourCommentSubmitted");
            }
            else
            {
                TempData["SuccessNotification"] = _localizationService.GetResource("YourCommentSubmittedAndPublishAfterReview");
            }
             
            return RedirectToAction("CommentEditor", new { postId = model.PostId });
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public virtual async Task<ActionResult> DeleteComment(int commentId)
        {
            await _commentsService.DeleteAsync(commentId);
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public virtual async Task<ActionResult> SetCommentStatus(int commentId, bool published)
        {
            var comment = await _commentsService.SetCommentPublished(commentId, published);           
            return View("Partials/_CommentBody", _commentModelFactory.PrepareCommentBodyModel(comment, true));
        }
    }
}