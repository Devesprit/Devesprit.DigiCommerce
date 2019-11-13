using Devesprit.Core.Settings;
using Devesprit.WebFramework.Attributes;

namespace Plugin.ExternalLogin.Models
{
    public partial class ExternalLoginProviderSettingsModel : ISettings
    {
        [DisplayNameLocalized("Plugin.ExternalLogin.EnableLoginWithGoogle")]
        public bool LoginWithGoogle { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.ExternalLogin.GoogleClientId")]
        public string GoogleLoginClientId { get; set; } = "00000";
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.ExternalLogin.GoogleClientSecret")]
        public string GoogleLoginClientSecret { get; set; } = "00000";
        [RequiredLocalized()]
        [DisplayNameLocalized("DisplayOrder")]
        public int GoogleDisplayOrder { get; set; }


        [DisplayNameLocalized("Plugin.ExternalLogin.EnableLoginWithFacebook")]
        public bool LoginWithFacebook { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.ExternalLogin.FacebookAppId")]
        public string FacebookLoginAppId { get; set; } = "00000";
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.ExternalLogin.FacebookAppSecret")]
        public string FacebookLoginAppSecret { get; set; } = "00000";
        [RequiredLocalized()]
        [DisplayNameLocalized("DisplayOrder")]
        public int FacebookDisplayOrder { get; set; }


        [DisplayNameLocalized("Plugin.ExternalLogin.EnableLoginWithTwitter")]
        public bool LoginWithTwitter { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.ExternalLogin.TwitterConsumerKey")]
        public string TwitterLoginConsumerKey { get; set; } = "00000";
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.ExternalLogin.TwitterConsumerSecret")]
        public string TwitterLoginConsumerSecret { get; set; } = "00000";
        [RequiredLocalized()]
        [DisplayNameLocalized("DisplayOrder")]
        public int TwitterDisplayOrder { get; set; }


        [DisplayNameLocalized("Plugin.ExternalLogin.UseProxy")]
        public bool UseProxy { get; set; }
        [DisplayNameLocalized("Plugin.ExternalLogin.ProxyServerAddress")]
        public string ProxyServerAddress { get; set; }
        [DisplayNameLocalized("Plugin.ExternalLogin.ProxyServerUserName")]
        public string ProxyServerUserName { get; set; }
        [DisplayNameLocalized("Plugin.ExternalLogin.ProxyServerPassword")]
        public string ProxyServerPassword { get; set; }

    }
}
