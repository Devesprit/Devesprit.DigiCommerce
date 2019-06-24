using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models.Users;
using Devesprit.Services.Countries;
using Devesprit.Services.EMail;
using Devesprit.Services.ExternalLoginProvider;
using Devesprit.Services.Users;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework;
using Devesprit.WebFramework.ActionFilters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using reCaptcha;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class UserController : BaseController
    {
        private readonly ICountriesService _countriesService;
        private readonly IEmailService _emailService;
        private readonly ILocalizationService _localizationService;
        private readonly IUsersService _usersService;
        private readonly IExternalLoginProviderManager _externalLoginProviderManager;

        private int FailedAttempts
        {
            get
            {
                int attempts;
                if (!int.TryParse(Session["FailedAttempts"]?.ToString(), out attempts))
                {
                    Session["FailedAttempts"] = "0";
                }

                return attempts;
            }
        }

        public UserController(ICountriesService countriesService, 
            IEmailService emailService, 
            ILocalizationService localizationService,
            IUsersService usersService,
            IExternalLoginProviderManager externalLoginProviderManager)
        {
            _countriesService = countriesService;
            _emailService = emailService;
            _localizationService = localizationService;
            _usersService = usersService;
            _externalLoginProviderManager = externalLoginProviderManager;
        }

        [HttpGet]
        [AllowAnonymous]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual ActionResult Login(string returnUrl)
        {
            Session["EnableExternalAuth"] = true;
            var currentLanguage = WorkContext.CurrentLanguage;
            if (CurrentSettings.UseGoogleRecaptchaForLogin && CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
            }

            return View(new LoginModel()
            {
                CurrentLanguage = currentLanguage,
                ReturnUrl = returnUrl,
                ExternalLoginProviders = _externalLoginProviderManager.GetAvailableLoginProvidersInfo()
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual async Task<ActionResult> Login(LoginModel model)
        {
            var currentLanguage = WorkContext.CurrentLanguage;
            model.CurrentLanguage = currentLanguage;
            model.ExternalLoginProviders = _externalLoginProviderManager.GetAvailableLoginProvidersInfo();

            if (CurrentSettings.UseGoogleRecaptchaForLogin && CurrentSettings.ShowRecaptchaAfterNFailedAttempt - 1 <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;


                if (CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
                {
                    if (!ReCaptcha.Validate(CurrentSettings.GoogleRecaptchaSecretKey))
                    {
                        IncreaseFailedAttempts();
                        ViewBag.RecaptchaLastErrors = ReCaptcha.GetLastErrors(HttpContext);
                        return View(model);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                IncreaseFailedAttempts();
                return View(model);
            }

            var user = UserManager.FindByEmail(model.Email);
            if (CurrentSettings.ConfirmUserEmailAddress)
            {
                if (user != null && !UserManager.IsEmailConfirmed(user.Id))
                {
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "User", new { userId = user.Id, code = code },
                        Request.Url.Scheme);
                    await _emailService.SendEmailFromTemplateAsync("ConfirmEmail", _localizationService.GetResource("ConfirmAccount"),
                        model.Email, new { Url = callbackUrl });
                    return View("DisplayEmailConfirm");
                }
            }

            var signIn = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                CurrentSettings.UserLockoutEnabled);
            
            switch (signIn)
            {
                case SignInStatus.Success:
                    EventPublisher.Publish(new UserLoggedinEvent(user));
                    _usersService.SetUserLatestIpAndLoginDate(user?.Id, HttpContext.GetClientIpAddress());
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    IncreaseFailedAttempts();
                    return View("Lockout");
                default:
                    IncreaseFailedAttempts();
                    ModelState.AddModelError("", _localizationService.GetResource("InvalidLogin"));
                    return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual async Task<ActionResult> SignUp()
        {
            Session["EnableExternalAuth"] = true;
            var currentLanguage = WorkContext.CurrentLanguage;
            var countries = await _countriesService.GetAsSelectListAsync();

            if (CurrentSettings.UseGoogleRecaptchaForSignup && CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
            }

            return View(new SignUpModel()
            {
                CurrentLanguage = currentLanguage,
                UserMustAcceptTerms = CurrentSettings.ShowAcceptTermsSignUp,
                CountriesList = countries,
                ExternalLoginProviders = _externalLoginProviderManager.GetAvailableLoginProvidersInfo()
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual async Task<ActionResult> SignUp(SignUpModel model)
        {
            var currentLanguage = WorkContext.CurrentLanguage;
            var countries = await _countriesService.GetAsSelectListAsync();

            model.UserMustAcceptTerms = CurrentSettings.ShowAcceptTermsSignUp;
            model.CurrentLanguage = currentLanguage;
            model.CountriesList = countries;
            model.ExternalLoginProviders = _externalLoginProviderManager.GetAvailableLoginProvidersInfo();

            if (CurrentSettings.UseGoogleRecaptchaForSignup && CurrentSettings.ShowRecaptchaAfterNFailedAttempt - 1 <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;

                if (CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
                {
                    if (!ReCaptcha.Validate(CurrentSettings.GoogleRecaptchaSecretKey))
                    {
                        IncreaseFailedAttempts();
                        ViewBag.RecaptchaLastErrors = ReCaptcha.GetLastErrors(HttpContext);
                        return View(model);
                    }
                }
            }

            if (!CurrentSettings.ShowAcceptTermsSignUp)
            {
                ModelState.Remove("AcceptTerms");
            }
            if (!ModelState.IsValid)
            {
                IncreaseFailedAttempts();
                return View(model);
            }

            var duplicatedEmail = await UserManager.FindByEmailAsync(model.Email);
            if (duplicatedEmail != null)
            {
                ModelState.AddModelError("Email", string.Format(_localizationService.GetResource("DuplicateEmail"), model.Email));
                IncreaseFailedAttempts();
                return View(model);
            }

            var user = new TblUsers()
            {
                UserName = model.Email,
                Email = model.Email,
                RegisterDate = DateTime.Now,
                FirstName = model.FName,
                LastName = model.LName,
                UserCountryId = model.Country,
            };

            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                if (CurrentSettings.ConfirmUserEmailAddress)
                {
                    var callbackUrl = Url.Action("ConfirmEmail", "User", new { userId = user.Id, code = code }, Request.Url.Scheme);
                    await _emailService.SendEmailFromTemplateAsync("ConfirmEmail", _localizationService.GetResource("ConfirmAccount"), model.Email, new { Url = callbackUrl });
                    return View("DisplayEmailConfirm");
                }

                var confirmEmailResult = await UserManager.ConfirmEmailAsync(user.Id, code);
                return View(confirmEmailResult.Succeeded ? "RegistrationCompleted" : "ErrorMessage");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
            IncreaseFailedAttempts();
            return View(model);
        }

        [AllowAnonymous]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "RegistrationCompleted" : "ErrorMessage");
        }

        [HttpGet]
        [AllowAnonymous]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual ActionResult ForgotPassword()
        {
            var currentLanguage = WorkContext.CurrentLanguage;

            if (CurrentSettings.UseGoogleRecaptchaForResetPassword && CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
            }

            return View(new ForgotPasswordModel()
            {
                CurrentLanguage = currentLanguage,
                Settings = CurrentSettings,
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual async Task<ActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            var currentLanguage = WorkContext.CurrentLanguage;
            model.Settings = CurrentSettings;
            model.CurrentLanguage = currentLanguage;

            if (CurrentSettings.UseGoogleRecaptchaForResetPassword && CurrentSettings.ShowRecaptchaAfterNFailedAttempt - 1 <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;


                if (CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
                {
                    if (!ReCaptcha.Validate(CurrentSettings.GoogleRecaptchaSecretKey))
                    {
                        IncreaseFailedAttempts();
                        ViewBag.RecaptchaLastErrors = ReCaptcha.GetLastErrors(HttpContext);
                        return View(model);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    IncreaseFailedAttempts();
                    ModelState.AddModelError("Email", _localizationService.GetResource("UserNotExist"));
                    return View(model);
                }

                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "User", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await _emailService.SendEmailFromTemplateAsync("ResetPassword", _localizationService.GetResource("ResetPassword"), model.Email, new { Url = callbackUrl });
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            IncreaseFailedAttempts();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual ActionResult ResetPassword(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(userId))
            {
                IncreaseFailedAttempts();
                return View("Error");
            }

            var currentLanguage = WorkContext.CurrentLanguage;

            if (CurrentSettings.UseGoogleRecaptchaForResetPassword && CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
            }

            return View(new ResetPasswordModel()
            {
                CurrentLanguage = currentLanguage,
                Settings = CurrentSettings
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual async Task<ActionResult> ResetPassword(ResetPasswordModel model)
        {
            var currentLanguage = WorkContext.CurrentLanguage;
            model.Settings = CurrentSettings;
            model.CurrentLanguage = currentLanguage;

            if (CurrentSettings.UseGoogleRecaptchaForResetPassword && CurrentSettings.ShowRecaptchaAfterNFailedAttempt - 1 <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;


                if (CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
                {
                    if (!ReCaptcha.Validate(CurrentSettings.GoogleRecaptchaSecretKey))
                    {
                        IncreaseFailedAttempts();
                        ViewBag.RecaptchaLastErrors = ReCaptcha.GetLastErrors(HttpContext);
                        return View(model);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    IncreaseFailedAttempts();
                    ModelState.AddModelError("Email", _localizationService.GetResource("UserNotExist"));
                    return View(model);
                }

                var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return View("ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    if (error.ToLower().Trim() == "Invalid token.".ToLower())
                    {
                        ModelState.AddModelError("", _localizationService.GetResource("InvalidToken"));
                    }
                    else
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            IncreaseFailedAttempts();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "User", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        [RedirectAuthenticatedRequests(Action = "Index", Controller = "Profile")]
        public virtual async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindByNameAsync(loginInfo.Email);
            if (user != null)
            {
                await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
            }
            var result = await SignInManager.ExternalSignInAsync(loginInfo, true);
            switch (result)
            {
                case SignInStatus.Success:
                    EventPublisher.Publish(new UserLoggedinEvent(user));
                    _usersService.SetUserLatestIpAndLoginDate(user?.Id, HttpContext.GetClientIpAddress());
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    IncreaseFailedAttempts();
                    return View("Lockout");
                default:
                    {
                        // If the user does not have an account, then prompt the user to create an account
                        var currentLanguage = WorkContext.CurrentLanguage;
                        var countries = await _countriesService.GetAsSelectListAsync();

                        if (CurrentSettings.UseGoogleRecaptchaForSignup && CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
                        {
                            ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;
                        }

                        var provider = _externalLoginProviderManager.FindByProviderName(loginInfo.Login.LoginProvider);
                        if (provider!= null)
                        {
                            var loginResult = provider.GetUserInformation(loginInfo);
                            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationModel
                            {
                                Email = loginInfo.Email,
                                FName = loginResult.UserFirstName,
                                LName = loginResult.UserLastName,
                                Country = loginResult.UserCountryId,
                                Avatar = loginResult.UserAvatarUrl,
                                CurrentLanguage = currentLanguage,
                                LoginProvider = loginInfo.Login.LoginProvider,
                                ReturnUrl = returnUrl,
                                Settings = CurrentSettings,
                                CountriesList = countries
                            });
                        }

                        return RedirectToAction("Login");
                    }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            var currentLanguage = WorkContext.CurrentLanguage;
            var countries = await _countriesService.GetAsSelectListAsync();

            model.Settings = CurrentSettings;
            model.CurrentLanguage = currentLanguage;
            model.CountriesList = countries;

            if (CurrentSettings.UseGoogleRecaptchaForSignup && CurrentSettings.ShowRecaptchaAfterNFailedAttempt - 1 <= FailedAttempts)
            {
                ViewBag.publicKey = CurrentSettings.GoogleRecaptchaSiteKey;

                if (CurrentSettings.ShowRecaptchaAfterNFailedAttempt <= FailedAttempts)
                {
                    if (!ReCaptcha.Validate(CurrentSettings.GoogleRecaptchaSecretKey))
                    {
                        IncreaseFailedAttempts();
                        ViewBag.RecaptchaLastErrors = ReCaptcha.GetLastErrors(HttpContext);
                        return View(model);
                    }
                }
            }

            if (!CurrentSettings.ShowAcceptTermsSignUp)
            {
                ModelState.Remove("AcceptTerms");
            }

            if (!ModelState.IsValid)
            {
                IncreaseFailedAttempts();
                return View(model);
            }

            var duplicatedEmail = await UserManager.FindByEmailAsync(model.Email);
            if (duplicatedEmail != null)
            {
                ModelState.AddModelError("Email", string.Format(_localizationService.GetResource("DuplicateEmail"), model.Email));
                IncreaseFailedAttempts();
                return View(model);
            }


            // Get the information about the user from the external login provider
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return View("Error");
            }

            var user = new TblUsers()
            {
                UserName = loginInfo.Email,
                Email = loginInfo.Email,
                RegisterDate = DateTime.Now,
                FirstName = model.FName,
                LastName = model.LName,
                UserCountryId = model.Country,
                EmailConfirmed = true,
                Avatar = (await DownloadUserAvatar(model.Avatar)).SaveToAppData("socialAvatar.png")
            };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                result = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);
                    EventPublisher.Publish(new UserLoggedinEvent(user));
                    _usersService.SetUserLatestIpAndLoginDate(user?.Id, HttpContext.GetClientIpAddress());
                    return RedirectToLocal(model.ReturnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
            IncreaseFailedAttempts();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> LogOff()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            await UserManager.UpdateSecurityStampAsync(User.Identity.GetUserId());
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignOut();
            Session["NotificationsAlert"] = null;
            EventPublisher.Publish(new UserLoggedOutEvent(user));
            return RedirectToAction("Index", "Home");
        }

        public virtual async Task<ActionResult> AccountDisabled()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            ViewBag.DisableReason = user?.DisableReason ?? "";
            return View();
        }

        #region Helpers

        protected virtual async Task<byte[]> DownloadUserAvatar(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    // Download the Web resource and save it into the current filesystem folder.
                    var imageBytes = await webClient.DownloadDataTaskAsync(url);

                    return CreateImageThumbnail(imageBytes);
                }
            }
            catch
            {
                return null;
            }
        }

        public byte[] CreateImageThumbnail(byte[] image, int width = 150, int height = 150)
        {
            using (var stream = new System.IO.MemoryStream(image))
            {
                var img = Image.FromStream(stream);
                var thumbnail = img.GetThumbnailImage(width, height, () => false, IntPtr.Zero);

                using (var thumbStream = new System.IO.MemoryStream())
                {
                    thumbnail.Save(thumbStream, System.Drawing.Imaging.ImageFormat.Png);
                    return thumbStream.GetBuffer();
                }
            }
        }

        protected virtual ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        protected virtual void IncreaseFailedAttempts()
        {
            int attempts;
            if (!int.TryParse(Session["FailedAttempts"]?.ToString(), out attempts))
            {
                Session["FailedAttempts"] = "0";
            }
            else
            {
                Session["FailedAttempts"] = attempts + 1;
            }
        }

        protected class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
            }
            
            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}