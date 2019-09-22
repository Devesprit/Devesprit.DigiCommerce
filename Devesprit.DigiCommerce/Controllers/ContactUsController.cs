using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models.ContactUs;
using Devesprit.Services.Users;
using reCaptcha;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class ContactUsController : BaseController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IUserMessagingService _userMessagingService;
        
        public ContactUsController(ILocalizationService localizationService, 
            IUserMessagingService userMessagingService)
        {
            _localizationService = localizationService;
            _userMessagingService = userMessagingService;
        }

        public virtual ActionResult Index()
        {
            if (CurrentSettings.UseGoogleRecaptchaForContactUs)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
            }

            return View(new ContactUsModel()
            {
                Email = WorkContext.CurrentUser?.Email,
                Name = (WorkContext.CurrentUser?.FirstName + " " + WorkContext.CurrentUser?.LastName).Trim()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(ContactUsModel model)
        {
            if (CurrentSettings.UseGoogleRecaptchaForContactUs)
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

            await _userMessagingService.AddAsync(new TblUserMessages()
            {
                Email = model.Email,
                Message = model.Message,
                Name = model.Name,
                ReceiveDate = DateTime.Now,
                Subject = model.Subject,
                UserId = WorkContext.CurrentUser?.Id
            });

            SuccessNotification(_localizationService.GetResource("YourMessageHasBeenReceived"));

            return RedirectToAction("Index");
        }
    }
}