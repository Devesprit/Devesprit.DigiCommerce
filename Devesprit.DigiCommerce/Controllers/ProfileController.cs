using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Profile;
using Devesprit.Services.Comments;
using Devesprit.Services.Countries;
using Devesprit.Services.EMail;
using Devesprit.Services.Notifications;
using Devesprit.Services.Users;
using Devesprit.WebFramework;
using Microsoft.AspNet.Identity;
using X.PagedList;

namespace Devesprit.DigiCommerce.Controllers
{
    [Authorize]
    public partial class ProfileController : BaseController
    {
        private readonly IProfileModelFactory _profileModelFactory;
        private readonly ICountriesService _countriesService;
        private readonly ICommentsService _commentsService;
        private readonly IEmailService _emailService;
        private readonly IUsersService _usersService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationsService _notificationsService;

        public ProfileController(IProfileModelFactory profileModelFactory, 
            ICountriesService countriesService,
            ICommentsService commentsService,
            IEmailService emailService,
            IUsersService usersService,
            ILocalizationService localizationService,
            INotificationsService notificationsService)
        {
            _profileModelFactory = profileModelFactory;
            _countriesService = countriesService;
            _commentsService = commentsService;
            _emailService = emailService;
            _usersService = usersService;
            _localizationService = localizationService;
            _notificationsService = notificationsService;
        }

        [HttpGet]
        public virtual async Task<ActionResult> Index(string userId)
        {
            TblUsers user;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                user = await UserManager.FindByIdAsync(userId);
                if (user != null)
                {
                    return View(await _profileModelFactory.PrepareProfileModelAsync(user));
                }
            }

            user = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            return View(await _profileModelFactory.PrepareProfileModelAsync(user));
        }


        [HttpGet]
        public virtual async Task<ActionResult> UpdateProfile(string userId)
        {
            TblUsers user = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                user = await UserManager.FindByIdAsync(userId);
            }
            else
            {
                user = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            }

            var model = _profileModelFactory.PrepareUpdateProfileModel(user);
            model.CountriesList = await _countriesService.GetAsSelectListAsync();
            model.MustConfirmNewEmail = CurrentSettings.ConfirmUserEmailAddress;
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> UpdateProfile(UpdateProfileModel model)
        {
            string currentUserId = null;
            if (!string.IsNullOrWhiteSpace(model.Id) && User.IsInRole("Admin"))
            {
                currentUserId = model.Id;
            }
            else
            {
                currentUserId = HttpContext.User.Identity.GetUserId();
            }

            model.CountriesList = await _countriesService.GetAsSelectListAsync();
            model.MustConfirmNewEmail = CurrentSettings.ConfirmUserEmailAddress;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var duplicatedEmail = await UserManager.FindByEmailAsync(model.Email);
            if (duplicatedEmail != null && duplicatedEmail.Id != currentUserId)
            {
                ModelState.AddModelError("Email", string.Format(_localizationService.GetResource("DuplicateEmail"), model.Email));
                return View(model);
            }

            var user = await UserManager.FindByIdAsync(currentUserId);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserCountryId = model.UserCountryId;
            if (model.Avatar != null)
            {
                try
                {
                    var oldAvatarFile = Server.MapPath(user.Avatar);
                    if (System.IO.File.Exists(oldAvatarFile))
                    {
                        System.IO.File.Delete(oldAvatarFile);
                    }
                }
                catch
                {}
                user.Avatar = model.Avatar.SaveToAppData();
            }

            if (string.Compare(user.Email.Trim(), model.Email.Trim(), StringComparison.OrdinalIgnoreCase) != 0)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
                user.EmailConfirmed = false;
            }

            var result = await UserManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (CurrentSettings.ConfirmUserEmailAddress && !user.EmailConfirmed)
                {
                    AuthenticationManager.SignOut();
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "User", new { userId = user.Id, code = code }, Request.Url.Scheme);
                    await _emailService.SendEmailFromTemplateAsync("ConfirmEmail", _localizationService.GetResource("ConfirmAccount"), model.Email, new { Url = callbackUrl });
                    return RedirectToAction("Login", "User");
                }

                return RedirectToAction("Index", new {userId = model.Id});
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }


        [HttpGet]
        public virtual async Task<ActionResult> ChangePassword(string userId)
        {
            TblUsers user = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                user = await UserManager.FindByIdAsync(userId);
            }
            else
            {
                user = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            }

            return View(new ChangePasswordModel(){Id = user.Id});
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            string currentUserId = null;
            if (!string.IsNullOrWhiteSpace(model.Id) && User.IsInRole("Admin"))
            {
                currentUserId = model.Id;
            }
            else
            {
                currentUserId = HttpContext.User.Identity.GetUserId();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByIdAsync(currentUserId);
            var passwordCheck = await UserManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
            {
                ModelState.AddModelError("CurrentPassword", _localizationService.GetResource("CurrentPasswordInvalid"));
                return View(model);
            }

            var result = await UserManager.ChangePasswordAsync(currentUserId, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { userId = model.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(model);
        }

        [HttpPost]
        public virtual async Task<ActionResult> UserInvoices(string userId, int? pageNumber)
        {
            IPagedList<TblInvoices> result = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                result = await _usersService.GetUserInvoicesAsync(userId, pageNumber ?? 1, 20, false);
            }
            else
            {
                result = await _usersService.GetUserInvoicesAsync(User.Identity.GetUserId(), pageNumber ?? 1, 20);
            }

            return View("Partials/_UserInvoices", result);
        }

        [HttpPost]
        public virtual async Task<ActionResult> UserLikes(string userId, int? pageNumber)
        {
            IPagedList<TblUserLikes> result = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                result = await _usersService.GetUserLikedPostsAsync(userId, pageNumber ?? 1, 20);
            }
            else
            {
                result = await _usersService.GetUserLikedPostsAsync(User.Identity.GetUserId(), pageNumber ?? 1, 20);
            }
            return View("Partials/_UserLikes", await _profileModelFactory.PrepareUserLikedEntitiesModelAsync(result));
        }

        [HttpPost]
        public virtual async Task<ActionResult> UserWishList(string userId, int? pageNumber)
        {
            IPagedList<TblUserWishlist> result = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                result = await _usersService.GetUserWishlistAsync(userId, pageNumber ?? 1, 20);
            }
            else
            {
                result = await _usersService.GetUserWishlistAsync(User.Identity.GetUserId(), pageNumber ?? 1, 20);
            }

            return View("Partials/_UserWishlist", await _profileModelFactory.PrepareUserWishlistModelAsync(result));
        }

        [HttpPost]
        public virtual async Task<ActionResult> UserDownloadLogs(string userId, int? pageNumber)
        {
            IPagedList<TblProductDownloadsLog> result = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                result = await _usersService.GetUserDownloadsLogAsync(userId, pageNumber ?? 1, 20);
            }
            else
            {
                result = await _usersService.GetUserDownloadsLogAsync(User.Identity.GetUserId(),
                    pageNumber ?? 1, 20);
            }

            return View("Partials/_UserDownloadsLog", result);
        }

        [HttpPost]
        public virtual async Task<ActionResult> UserNotifications(string userId, int? pageNumber)
        {
            IPagedList<TblNotifications> result = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                result = await _notificationsService.GetUserNotificationsAsPagedListAsync(userId,
                    userId == User.Identity.GetUserId(), pageNumber ?? 1, 20);
            }
            else
            {
                result = await _notificationsService.GetUserNotificationsAsPagedListAsync(User.Identity.GetUserId(), true,
                    pageNumber ?? 1, 20);
            }

            return View("Partials/_UserNotifications", result);
        }

        [HttpPost]
        public virtual async Task<ActionResult> UserComments(string userId, int? pageNumber)
        {
            IPagedList<TblPostComments> result = null;
            if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin"))
            {
                result = await _commentsService.GetUserCommentsAsPagedListAsync(userId, pageNumber ?? 1, 20);
            }
            else
            {
                result = await _commentsService.GetUserCommentsAsPagedListAsync(User.Identity.GetUserId(), pageNumber ?? 1, 20);
            }

            return View("Partials/_UserComments", result);
        }

        [HttpPost]
        public virtual async Task<ActionResult> DeleteNotification(int id)
        {
            if (User.IsInRole("Admin"))
            {
                await _notificationsService.DeleteAsync(id);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(User.Identity.GetUserId()))
                {
                    var notification = await _notificationsService.FindByIdAsync(id);
                    if (notification.UserId == User.Identity.GetUserId())
                    {
                        await _notificationsService.DeleteAsync(id);
                    }
                }
            }

            return Json(new { response = "OK" });
        }
    }
}