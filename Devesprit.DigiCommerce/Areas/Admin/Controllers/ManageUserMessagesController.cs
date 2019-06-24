using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Users;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageUserMessagesController : BaseController
    {
        private readonly IUserMessagingService _userMessagingService;
        private readonly IUserMessagesModelFactory _userMessagesModelFactory;
        private readonly ILocalizationService _localizationService;
        
        public ManageUserMessagesController(IUserMessagingService userMessagingService,
            IUserMessagesModelFactory userMessagesModelFactory,
            ILocalizationService localizationService)
        {
            _userMessagingService = userMessagingService;
            _userMessagesModelFactory = userMessagesModelFactory;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual async Task<ActionResult> ReplyToUserMessage(int id)
        {
            var record = await _userMessagingService.FindByIdAsync(id);
            if (record != null)
            {
                return View(_userMessagesModelFactory.PrepareReplyToUserMessageModel(record));
            }

            return RedirectToAction("PageNotFound", "Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ReplyToUserMessage(ReplyToUserMessageModel model, bool? saveAndSendReply)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = await _userMessagingService.FindByIdAsync(model.Id);
            try
            {
                if (!model.ResponseText.IsNullOrWhiteSpace())
                {
                    record.ResponseText = model.ResponseText;
                }
                
                record.Subject = model.Subject;
                record.Email = model.Email;
                record.Readed = true;

                await _userMessagingService.UpdateAsync(record);

                if (saveAndSendReply != null && saveAndSendReply.Value && !model.ResponseText.IsNullOrWhiteSpace())
                {
                    await _userMessagingService.ReplyToMessage(record.Id, model.ResponseText);
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
                                window.opener.refreshUserMessagesGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _userMessagingService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult> SetMessageStatus(int id, bool readed)
        {
            try
            {
                if (readed)
                {
                    await _userMessagingService.SetAsReaded(id);
                }
                else
                {
                    await _userMessagingService.SetAsUnReaded(id);
                }
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm)
        {
            var query = _userMessagingService.GetAsQueryable();

            var url = Url.Action("Editor", "ManageUsers", new { area = "Admin" });
            var dataSource = query.Select(p => new
            {
                p.Id,
                Name = p.User != null
                    ? "<a href=\"javascript:PopupWindows('" + url + "', 'UserEditor', 1200, 700, { id: '" + p.UserId +
                      "' }, 'get')\">" + p.User.Email + "</a>"
                    : p.Name,
                p.Email,
                p.Subject,
                p.Readed,
                p.Replied,
                p.ReceiveDate,
                p.ReplyDate
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}