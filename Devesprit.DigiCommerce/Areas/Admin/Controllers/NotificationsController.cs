using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Notifications;
using Devesprit.Services.TemplateEngine;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class NotificationsController : BaseController
    {
        private readonly INotificationsService _notificationsService;
        private readonly ITemplateEngine _templateEngine;
        private readonly ILocalizationService _localizationService;

        public NotificationsController(
            INotificationsService notificationsService,
            ITemplateEngine templateEngine,
            ILocalizationService localizationService)
        {
            _notificationsService = notificationsService;
            _templateEngine = templateEngine;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index(bool? seeUsersNotifications)
        {
            ViewBag.SeeUsersNotifications = seeUsersNotifications == true;
            return View();
        }

        public virtual ActionResult SendMessageToUser(string userEmail)
        {
            return View(new SendMessageModel(){Recipient = userEmail });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> SendMessageToUser(SendMessageModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await UserManager.FindByEmailAsync(model.Recipient);
                if (user != null)
                {
                    await _notificationsService.SendNotificationAsync(user.Id, model.Message, null, false);
                }
                else
                {
                    ModelState.AddModelError("", _localizationService.GetResource("RequestedUserNotFound"));
                    return View(model);
                }
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshNotificationsGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _notificationsService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult> SetNotificationStatus(int id, bool readed)
        {
            try
            {
                var record = await _notificationsService.FindByIdAsync(id);
                if (record != null)
                {
                    record.Readed = readed;
                    await _notificationsService.UpdateAsync(record);
                }
                
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm, bool seeUsersNotifications)
        {
            var query = _notificationsService.GetAsQueryable();

            query = seeUsersNotifications
                ? query.Where(p => p.UserId != null)
                : query.Where(p => p.UserId == null);

            var url = Url.Action("Editor", "ManageUsers", new {area = "Admin"});
            var dataSource = query.Select(p => new
            {
                p.Id,
                Date = p.NotificationDate,
                p.Readed,
                p.IsMessage,
                p.UserId,
                UserEmail = p.User.Email ?? "-",
                Recipient = p.User != null
                    ? "<a href=\"javascript:PopupWindows('" + url + "', 'UserEditor', 1200, 700, { id: '" + p.UserId +
                      "' }, 'get')\">" + p.User.Email + "</a>"
                    : "Admin",
                p.MessageResourceName,
                p.MessageArguments
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList().Select(p => new
            {
                p.Id,
                p.Date,
                p.Readed,
                p.IsMessage,
                p.UserId,
                p.UserEmail,
                p.Recipient,
                Message = p.IsMessage
                    ? p.MessageResourceName
                    : _templateEngine.CompileTemplate(_localizationService.GetResource(p.MessageResourceName),
                        p.MessageArguments.JsonToObject())
            });

            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetAdminUnreadedNotificationsCount()
        {
            var notificationCount = _notificationsService.GetUserUnReadedNotificationsCount(null);
            if (notificationCount == 0)
            {
                return Content("");
            }
            return Content($"<span class='badge badge-danger'>{notificationCount}</span>");
        }
    }
}