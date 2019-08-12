using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Services;
using Devesprit.Services.ThemeManager;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class SiteSettingModel
    {
        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("SiteUrl")]
        public string SiteUrl { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [AllowHtml]
        [DisplayNameLocalized("SiteName")]
        public LocalizedString SiteName { get; set; } = new LocalizedString();

        [DisplayNameLocalized("InvoicePrintPageCompanyInfo")]
        [AllowHtml]
        public LocalizedString InvoicePrintPageCompanyInfo { get; set; } = new LocalizedString();


        [MaxLengthLocalized(1000)]
        [DisplayNameLocalized("SiteDescription")]
        public LocalizedString SiteDescription { get; set; } = new LocalizedString();


        [DisplayNameLocalized("MetaKeyword")]
        public LocalizedString MetaKeyWords { get; set; } = new LocalizedString();


        [MaxLengthLocalized(1500)]
        [AllowHtml]
        [DisplayNameLocalized("FooterDescription")]
        public LocalizedString FooterDescription { get; set; } = new LocalizedString();

        [AllowHtml]
        [DisplayNameLocalized("ContactUsDescription")]
        public LocalizedString ContactUsDescription { get; set; } = new LocalizedString();

        [AllowHtml]
        [DisplayNameLocalized("TermsAndConditionsDescription")]
        public LocalizedString TermsAndConditions { get; set; } = new LocalizedString();

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("SiteLogoHeaderUrl")]
        public virtual LocalizedString SiteLogoHeader { get; set; } = new LocalizedString();

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("SiteLogoInvoicePrintUrl")]
        public virtual LocalizedString SiteLogoInvoicePrint { get; set; } = new LocalizedString();

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("SiteLogoNavigationBarUrl")]
        public virtual LocalizedString SiteLogoNavigationBar { get; set; } = new LocalizedString();

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("SiteLogoEmailHeaderUrl")]
        public virtual LocalizedString SiteLogoEmailHeader { get; set; } = new LocalizedString();

        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("FavIconUrl")]
        public virtual LocalizedString FavIcon { get; set; } = new LocalizedString();
        
        [DisplayNameLocalized("UseGoogleRecaptchaLogin")]
        public bool UseGoogleRecaptchaForLogin { get; set; }

        [DisplayNameLocalized("UseGoogleRecaptchaForContactUs")]
        public bool UseGoogleRecaptchaForContactUs { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("ShowRecaptchaAfterNFailedAttempt")]
        public int ShowRecaptchaAfterNFailedAttempt { get; set; }


        [DisplayNameLocalized("UseGoogleRecaptchaSignup")]
        public bool UseGoogleRecaptchaForSignup { get; set; }


        [DisplayNameLocalized("UseGoogleRecaptchaResetPassword")]
        public bool UseGoogleRecaptchaForResetPassword { get; set; }


        [DisplayNameLocalized("UseGoogleRecaptchaForComment")]
        public bool UseGoogleRecaptchaForComment { get; set; }


        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("GoogleRecaptchaSiteKey")]
        public string GoogleRecaptchaSiteKey { get; set; }


        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("GoogleRecaptchaSecretKey")]
        public string GoogleRecaptchaSecretKey { get; set; }


        [DisplayNameLocalized("UserLockoutEnabled")]
        public bool UserLockoutEnabled { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("AccountLockoutTime")]
        public int AccountLockoutTime { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("MaxFailedAccessAttemptsBeforeLockout")]
        public int MaxFailedAccessAttemptsBeforeLockout { get; set; }


        [DisplayNameLocalized("ShowAcceptTermsSignUp")]
        public bool ShowAcceptTermsSignUp { get; set; }


        [DisplayNameLocalized("ConfirmUserEmailAddressEnabled")]
        public bool ConfirmUserEmailAddress { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("PasswordRequiredLength")]
        public int PasswordRequiredLength { get; set; }


        [DisplayNameLocalized("PasswordMustHaveNonLetterOrDigit")]
        public bool PasswordRequireNonLetterOrDigit { get; set; }


        [DisplayNameLocalized("PasswordMustHaveDigit")]
        public bool PasswordRequireDigit { get; set; }


        [DisplayNameLocalized("PasswordMustHaveLowerCase")]
        public bool PasswordRequireLowercase { get; set; }


        [DisplayNameLocalized("PasswordMustHaveUpperCase")]
        public bool PasswordRequireUppercase { get; set; }


        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("SystemEmailAddress")]
        public string SiteEmailAddress { get; set; }


        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("SMTPServerAddress")]
        public string SMTPServer { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("SMTPPortNumber")]
        public int SMTPPort { get; set; }


        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("SMTPPassword")]
        public string SMTPPassword { get; set; }


        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("SMTPUserName")]
        public string SMTPUserName { get; set; }


        [DisplayNameLocalized("SMTPEnableSsl")]
        public bool SMTPEnableSsl { get; set; }


        [DisplayNameLocalized("GetUserBillingAddressOnCheckout")]
        public bool GetBillingAddressForInvoice { get; set; }


        [DisplayNameLocalized("ShowQtyColInInvoice")]
        public bool ShowQtyColInInvoice { get; set; }


        [DisplayNameLocalized("UserMustRegisterBeforeCheckoutInvoice")]
        public bool UserMustRegisterBeforeCheckoutInvoice { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("DownloadUrlsAgeByHour")]
        public int DownloadUrlsAgeByHour { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("NumberOfDownloadUrlsFireLimit")]
        public int NumberOfDownloadUrlsFireLimit { get; set; }


        [MaxLengthLocalized(1000)]
        [DisplayNameLocalized("DownloadableFilesExtensions")]
        public string DownloadableFilesExtensions { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("EncryptionKey")]
        public string EncryptionKey { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("EncryptionSalt")]
        public string EncryptionSalt { get; set; }

        [AllowHtml]
        [DisplayNameLocalized("CkEditorConfig")]
        public string CkEditorConfig { get; set; }

        [AllowHtml]
        [DisplayNameLocalized("CkEditorTemplates")]
        public string CkEditorTemplates { get; set; }

        [DisplayNameLocalized("AutoPublishComments")]
        public bool AutoPublishComments { get; set; }

        [DisplayNameLocalized("AllowUsersToWriteComment")]
        public bool AllowUsersToWriteComment { get; set; }

        [DisplayNameLocalized("AllowGuestUsersToWriteComment")]
        public bool AllowGuestUsersToWriteComment { get; set; }

        [DisplayNameLocalized("EnableWishlist")]
        public bool Wishlist { get; set; }

        [DisplayNameLocalized("EnableLikePosts")]
        public bool LikePosts { get; set; }

        [DisplayNameLocalized("AppendLanguageCodeToUrl")]
        public bool AppendLanguageCodeToUrl { get; set; }

        public string WebsiteTheme { get; set; }

        [DisplayNameLocalized("EnableHtmlMinification")]
        public bool EnableHtmlMinification { get; set; }

        [DisplayNameLocalized("EnableResponseCompression")]
        public bool EnableResponseCompression { get; set; }

        [DisplayNameLocalized("EnableJsBundling")]
        public bool EnableJsBundling { get; set; }

        [DisplayNameLocalized("EnableInlineJsMinification")]
        public bool EnableInlineJsMinification { get; set; }

        [DisplayNameLocalized("EnableCssBundling")]
        public bool EnableCssBundling { get; set; }

        [DisplayNameLocalized("EnableInlineCssMinification")]
        public bool EnableInlineCssMinification { get; set; }

        [DisplayNameLocalized("BlogEnabled")]
        public bool EnableBlog { get; set; }

        [DisplayNameLocalized("DeleteEmptyInvoices")]
        public bool DeleteEmptyInvoices { get; set; }

        [DisplayNameLocalized("DeleteEmptyInvoicesAfterDays")]
        [RequiredLocalized()]
        public int DeleteEmptyInvoicesAfterDays { get; set; }

        [DisplayNameLocalized("DeletePendingInvoices")]
        public bool DeletePendingInvoices { get; set; }

        [DisplayNameLocalized("DeletePendingInvoicesAfterDays")]
        [RequiredLocalized()]
        public int DeletePendingInvoicesAfterDays { get; set; } 

        //Reminders Settings-------------------------
        [DisplayNameLocalized("SendExpNotificationsForProducts")]
        public virtual bool SendExpNotificationsForProducts { get; set; }
        [DisplayNameLocalized("SendExpNotificationsForProductsJustOnce")]
        public virtual bool SendExpNotificationsForProductsJustOnce { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("NotificationHourSpanBeforeExpForProducts")]
        public virtual int NotificationHourSpanBeforeExpForProducts { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("NotificationHourSpanAfterExpForProducts")]
        public virtual int NotificationHourSpanAfterExpForProducts { get; set; }
        [DisplayNameLocalized("SendExpNotificationsForProductAttributes")]
        public virtual bool SendExpNotificationsForProductAttributes { get; set; }
        [DisplayNameLocalized("SendExpNotificationsForProductAttributesJustOnce")]
        public virtual bool SendExpNotificationsForProductAttributesJustOnce { get; set; }


        [DisplayNameLocalized("SendExpNotificationsForUserPlans")]
        public virtual bool SendExpNotificationsForUserPlans { get; set; }
        [DisplayNameLocalized("SendExpNotificationsForUserPlansJustOnce")]
        public virtual bool SendExpNotificationsForUserPlansJustOnce { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("NotificationHourSpanBeforeExpForUserPlans")]
        public virtual int NotificationHourSpanBeforeExpForUserPlans { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("NotificationHourSpanAfterExpForUserPlans")]
        public virtual int NotificationHourSpanAfterExpForUserPlans { get; set; }


        [DisplayNameLocalized("SendExpNotificationsForInvoiceManualItems")]
        public virtual bool SendExpNotificationsForInvoiceManualItems { get; set; }
        [DisplayNameLocalized("SendExpNotificationsForInvoiceManualItemsJustOnce")]
        public virtual bool SendExpNotificationsForInvoiceManualItemsJustOnce { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("NotificationHourSpanBeforeExpForInvoiceManualItems")]
        public virtual int NotificationHourSpanBeforeExpForInvoiceManualItems { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("NotificationHourSpanAfterExpForInvoiceManualItems")]
        public virtual int NotificationHourSpanAfterExpForInvoiceManualItems { get; set; }
        //-------------------------Reminders Settings

        public List<ThemeConfiguration> ListOfInstalledThemes
        {
            get
            {
                var themeProvider = DependencyResolver.Current.GetService<IThemeProvider>();
                themeProvider.ReLoadInstalledThemes();
                return themeProvider.GetThemeConfigurations().ToList();
            }
        }
    }
}