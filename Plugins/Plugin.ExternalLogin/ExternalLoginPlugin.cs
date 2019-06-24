using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Services.ExternalLoginProvider;
using Devesprit.Services.Localization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;
using Newtonsoft.Json;
using Owin;
using Plugin.ExternalLogin.Models;

namespace Plugin.ExternalLogin
{
    public class ExternalLoginPlugin : BasePlugin, IExternalLoginProvider
    {
        private readonly ISettingService _settingService;

        public ExternalLoginPlugin(ISettingService settingsService)
        {
            _settingService = settingsService;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ExternalLoginProvider";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.ExternalLogin.Controllers"},
                { "area", null}
            };
        }

        public override void Install()
        {
            _settingService.SaveSetting(new ExternalLoginProviderSettingsModel());

            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithFacebook", "Enable login with Facebook account", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithTwitter", "Enable login with Twitter account", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithGoogle", "Enable login with Google account", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.GoogleClientId", "Google Client ID", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.GoogleClientSecret", "Google Client Secret", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.FacebookAppId", "Facebook App Id", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.FacebookAppSecret", "Facebook App Secret", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.TwitterConsumerKey", "Twitter Consumer Key", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.TwitterConsumerSecret", "Twitter Consumer Secret", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Configuration", "Social Login Configurations", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Twitter", "Twitter", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Google", "Google", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Facebook", "Facebook", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.AuthorizedRedirectURL", "<i class=\"fa fa-exclamation-triangle\"></i> Authorized redirect URL : ", "en");

            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithFacebook", "فعال سازی ورود توسط حساب کاربری فیسبوک", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithTwitter", "فعال سازی ورود توسط حساب کاربری توئیتر", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithGoogle", "فعال سازی ورود توسط حساب کاربری گوگل", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.GoogleClientId", "Google Client Id", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.GoogleClientSecret", "Google Client Secret", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.FacebookAppId", "Facebook App Id", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.FacebookAppSecret", "Facebook App Secret", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.TwitterConsumerKey", "Twitter Consumer Key", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.TwitterConsumerSecret", "Twitter Consumer Secret", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Configuration", "تنظیمات ورود توسط جوامع مجازی", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Twitter", "توئیتر", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Google", "گوگل", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.Facebook", "فیسبوک", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.ExternalLogin.AuthorizedRedirectURL", "<i class=\"fa fa-exclamation-triangle\"></i> Authorized redirect URL : ", "fa");

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<ExternalLoginProviderSettingsModel>();

            this.DeletePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithFacebook");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithTwitter");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.EnableLoginWithGoogle");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.GoogleClientId");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.GoogleClientSecret");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.FacebookAppId");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.FacebookAppSecret");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.TwitterConsumerKey");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.TwitterConsumerSecret");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.Configuration");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.Twitter");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.Google");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.Facebook");
            this.DeletePluginLocaleResource("Plugin.ExternalLogin.AuthorizedRedirectURL");
            base.Uninstall();
        }

        public void UseCustomLoginProvider(IAppBuilder app)
        {
            var setting = _settingService.LoadSetting<ExternalLoginProviderSettingsModel>();
            if (setting.LoginWithGoogle)
            {
                var googleOptions = new GoogleOAuth2AuthenticationOptions()
                {
                    ClientId = setting.GoogleLoginClientId,
                    ClientSecret = setting.GoogleLoginClientSecret,
                    Provider = new GoogleOAuth2AuthenticationProvider()
                    {
                        OnAuthenticated = (context) =>
                        {
                            context.Identity.AddClaim(new Claim("name",
                                context.Identity.FindFirstValue(ClaimTypes.Name)));
                            context.Identity.AddClaim(new Claim("email",
                                context.Identity.FindFirstValue(ClaimTypes.Email)));
                            //This following line is need to retrieve the profile image
                            context.Identity.AddClaim(new Claim("accesstoken",
                                context.AccessToken, ClaimValueTypes.String, "Google"));

                            return Task.FromResult(0);
                        }
                    }
                };
                app.UseGoogleAuthentication(googleOptions);
            }

            if (setting.LoginWithFacebook)
            {
                var facebookOptions = new FacebookAuthenticationOptions()
                {
                    AppId = setting.FacebookLoginAppId,
                    AppSecret = setting.FacebookLoginAppSecret,
                    Scope = { "email" },
                    UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,email,first_name,last_name",
                    Provider = new FacebookAuthenticationProvider()
                    {
                        OnAuthenticated = (context) =>
                        {
                            //This following line is need to retrieve the profile image
                            context.Identity.AddClaim(new Claim("accesstoken",
                                context.AccessToken, ClaimValueTypes.String, "Facebook"));

                            return Task.FromResult(0);
                        }
                    }
                };
                app.UseFacebookAuthentication(facebookOptions);
            }

            if (setting.LoginWithTwitter)
            {
                var twitterOptions = new TwitterAuthenticationOptions()
                {
                    ConsumerKey = setting.TwitterLoginConsumerKey,
                    ConsumerSecret = setting.TwitterLoginConsumerSecret,
                    Provider = new TwitterAuthenticationProvider()
                    {
                        OnAuthenticated = (context) =>
                        {
                            var res = TwitterLogin(context.AccessToken, context.AccessTokenSecret,
                                setting.TwitterLoginConsumerKey, setting.TwitterLoginConsumerSecret);
                            context.Identity.AddClaim(new Claim(ClaimTypes.Email, res.Email.Trim()));
                            context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, res.FirstName.Trim()));
                            context.Identity.AddClaim(new Claim(ClaimTypes.Surname, res.LastName.Trim()));
                            context.Identity.AddClaim(new Claim("avatar", res.ProfileImageUrl.Trim()));
                            return Task.FromResult(0);
                        }
                    }
                };
                app.UseTwitterAuthentication(twitterOptions);
            }
        }

        public ExternalLoginUserInformation GetUserInformation(ExternalLoginInfo loginInfo)
        {
            var result = new ExternalLoginUserInformation();
            if (loginInfo.Login.LoginProvider == "Google")
            {
                var externalIdentity = loginInfo.ExternalIdentity;
                var lastNameClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
                var givenNameClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
                var accessToken = externalIdentity.Claims.Where(c => c.Type.Equals("accesstoken")).Select(c => c.Value).FirstOrDefault();

                result.UserFirstName = givenNameClaim?.Value;
                result.UserLastName = lastNameClaim?.Value;
                try
                {
                    Uri apiRequestUri = new Uri("https://www.googleapis.com/oauth2/v2/userinfo?access_token=" + accessToken);
                    //request profile image
                    using (var webClient = new WebClient())
                    {
                        var json = webClient.DownloadString(apiRequestUri);
                        dynamic jsonObject = JsonConvert.DeserializeObject(json);
                        result.UserAvatarUrl = jsonObject.picture;
                    }
                }
                catch
                { }
            }

            if (loginInfo.Login.LoginProvider == "Facebook")
            {
                var accessToken = loginInfo.ExternalIdentity.Claims.Where(c => c.Type.Equals("accesstoken")).Select(c => c.Value).FirstOrDefault();
                try
                {
                    Uri apiRequestUri = new Uri("https://graph.facebook.com/v2.4/me?fields=id,email,first_name,last_name&access_token=" + accessToken);
                    //request profile image
                    using (var webClient = new WebClient())
                    {
                        var json = webClient.DownloadString(apiRequestUri);
                        dynamic jsonObject = JsonConvert.DeserializeObject(json);
                        result.UserAvatarUrl = string.Format("http://graph.facebook.com/{0}/picture?type=large", jsonObject.id);
                        result.UserFirstName = jsonObject.first_name;
                        result.UserLastName = jsonObject.last_name;
                    }
                }
                catch
                { }
            }

            if (loginInfo.Login.LoginProvider == "Twitter")
            {
                var externalIdentity = loginInfo.ExternalIdentity;
                var lastNameClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
                var givenNameClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
                var userPictureClaim = externalIdentity.Claims.FirstOrDefault(c => c.Type.Equals("avatar"));

                result.UserFirstName = givenNameClaim?.Value;
                result.UserLastName = lastNameClaim?.Value;
                result.UserAvatarUrl = userPictureClaim?.Value;
            }

            return result;
        }

        public List<ExternalLoginProviderInfo> GetProviders()
        {
            var result = new List<ExternalLoginProviderInfo>();
            var setting = _settingService.LoadSetting<ExternalLoginProviderSettingsModel>();
            if (setting.LoginWithGoogle)
            {
                result.Add(new ExternalLoginProviderInfo()
                {
                    ProviderName = "Google",
                    ProviderLoginBtnPartialUrl = "~/Plugins/Plugin.ExternalLogin/Views/Partials/_LoginWithGoogle.cshtml",
                    Order = setting.GoogleDisplayOrder
                });
            }
            if (setting.LoginWithFacebook)
            {
                result.Add(new ExternalLoginProviderInfo()
                {
                    ProviderName = "Facebook",
                    ProviderLoginBtnPartialUrl = "~/Plugins/Plugin.ExternalLogin/Views/Partials/_LoginWithFacebook.cshtml",
                    Order = setting.FacebookDisplayOrder
                });
            }
            if (setting.LoginWithTwitter)
            {
                result.Add(new ExternalLoginProviderInfo()
                {
                    ProviderName = "Twitter",
                    ProviderLoginBtnPartialUrl = "~/Plugins/Plugin.ExternalLogin/Views/Partials/_LoginWithTwitter.cshtml",
                    Order = setting.TwitterDisplayOrder
                });
            }

            return result;
        }

        public TwitterUserDetail TwitterLogin(string oauthToken, string oauthTokenSecret, string oauthConsumerKey, string oauthConsumerSecret)
        {
            // oauth implementation details
            const string oauthVersion = "1.0";
            const string oauthSignatureMethod = "HMAC-SHA1";

            // unique request details
            var oauthNonce = Convert.ToBase64String(
                new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.Now
                           - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauthTimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            var resourceUrl = "https://api.twitter.com/1.1/account/verify_credentials.json";
            const string requestQuery = "include_email=true";
            // create oauth signature
            const string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                                      "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";

            var baseString = string.Format(baseFormat,
                oauthConsumerKey,
                oauthNonce,
                oauthSignatureMethod,
                oauthTimestamp,
                oauthToken,
                oauthVersion
            );

            baseString = string.Concat("GET&", Uri.EscapeDataString(resourceUrl) + "&" + Uri.EscapeDataString(requestQuery), "%26", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(oauthConsumerSecret),
                "&", Uri.EscapeDataString(oauthTokenSecret));

            string oauthSignature;
            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                oauthSignature = Convert.ToBase64String(
                    hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            const string headerFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", oauth_timestamp=\"{4}\", oauth_token=\"{5}\", oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                Uri.EscapeDataString(oauthConsumerKey),
                Uri.EscapeDataString(oauthNonce),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(oauthSignatureMethod),
                Uri.EscapeDataString(oauthTimestamp),
                Uri.EscapeDataString(oauthToken),
                Uri.EscapeDataString(oauthVersion)
            );


            // make the request

            ServicePointManager.Expect100Continue = false;
            resourceUrl += "?include_email=true";
            var request = (HttpWebRequest)WebRequest.Create(resourceUrl);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "GET";

            var response = request.GetResponse();
            return JsonConvert.DeserializeObject<TwitterUserDetail>(new StreamReader(response.GetResponseStream()).ReadToEnd());
        }
    }
}
