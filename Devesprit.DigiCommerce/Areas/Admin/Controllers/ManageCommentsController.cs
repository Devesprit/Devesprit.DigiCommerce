using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Comments;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageComments")]
    public partial class ManageCommentsController : BaseController
    {
        private readonly ICommentsService _commentsService;
        private readonly ILocalizationService _localizationService;

        public ManageCommentsController(
            ICommentsService commentsService,
            ILocalizationService localizationService)
        {
            _commentsService = commentsService;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index(PostType? filterByPostType)
        {
            ViewBag.FilterByPostType = filterByPostType ?? PostType.Product;
            return View();
        }

        [HttpPost]
        [UserHasPermission("ManageComments_Delete")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _commentsService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        [UserHasPermission("ManageComments_PublishUnPublish")]
        public virtual async Task<ActionResult> SetCommentStatus(int commentId, bool published)
        {
            try
            {
                await _commentsService.SetCommentPublished(commentId, published);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current)
                    .Log(new Error(e, System.Web.HttpContext.Current));
                return Content(
                    string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [UserHasPermission("ManageComments_Edit")]
        public virtual async Task<ActionResult> Update(TblPostComments value)
        {
            var comment = await _commentsService.FindByIdAsync(value.Id);
            comment.Comment = value.Comment;
            comment.CommentDate = value.CommentDate;
            await _commentsService.UpdateAsync(comment);
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GridDataSource(DataManager dm, PostType filterByPostType)
        {
            var query = _commentsService.GetAsQueryable().Where(p=> p.Post.PostType == filterByPostType);
            var postUrl = filterByPostType == PostType.Product
                ? Url.Action("Index", "Product", new { area = "" })
                : Url.Action("Post", "Blog", new {area = ""});
            var dataSource = query.Select(p => new
            {
                p.Id,
                p.Comment,
                p.CommentDate,
                p.UserEmail,
                p.Published,
                PostTitle = "<a target='_blank' href='" + postUrl + "/" + p.Post.Slug + "'>" + p.Post.Title + "</a>"
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}