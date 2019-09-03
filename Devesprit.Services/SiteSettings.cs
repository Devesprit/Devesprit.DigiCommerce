using System;
using System.Collections.Generic;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;

namespace Devesprit.Services
{
    public partial class SiteSettings: ISettings
    {
        public virtual string SiteUrl { get; set; } = "http://www.mywebsite.com";
        public virtual LocalizedString SiteName { get; set; } = new LocalizedString("My Web Site");
        public virtual LocalizedString InvoicePrintPageCompanyInfo { get; set; } = new LocalizedString("My Web Site");
        public virtual LocalizedString ContactUsDescription { get; set; } = new LocalizedString();
        public virtual LocalizedString SiteDescription { get; set; } = new LocalizedString();
        public virtual LocalizedString TermsAndConditions { get; set; } = new LocalizedString();
        public virtual LocalizedString MetaKeyWords { get; set; } = new LocalizedString();
        public virtual LocalizedString FooterDescription { get; set; } = new LocalizedString();
        public virtual LocalizedString SiteLogoHeader { get; set; } = new LocalizedString("/Content/img/site-logo-header-white.png");
        public virtual LocalizedString SiteLogoEmailHeader { get; set; } = new LocalizedString("/Content/img/site-logo-email-header.png");
        public virtual LocalizedString SiteLogoNavigationBar { get; set; } = new LocalizedString("/Content/img/site-logo-navbar-black.png");
        public virtual LocalizedString SiteLogoInvoicePrint { get; set; } = new LocalizedString("/Content/img/site-logo-invoice-print.png");
        public virtual LocalizedString FavIcon { get; set; } = new LocalizedString("/Content/img/favicon.png");
        public virtual bool UseGoogleRecaptchaForLogin { get; set; }
        public virtual bool UseGoogleRecaptchaForContactUs { get; set; }
        public virtual int ShowRecaptchaAfterNFailedAttempt { get; set; } = 3;
        public virtual bool UseGoogleRecaptchaForSignup { get; set; }
        public virtual bool UseGoogleRecaptchaForResetPassword { get; set; }
        public virtual bool UseGoogleRecaptchaForComment { get; set; }
        public virtual string GoogleRecaptchaSiteKey { get; set; }
        public virtual string GoogleRecaptchaSecretKey { get; set; }
        public virtual bool AutoPublishComments { get; set; } = true;
        public virtual bool AllowUsersToWriteComment { get; set; } = true;
        public virtual bool AllowGuestUsersToWriteComment { get; set; } = true;
        public virtual bool UserLockoutEnabled { get; set; } = true;
        public virtual TimeSpan AccountLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
        public virtual int MaxFailedAccessAttemptsBeforeLockout { get; set; } = 5;
        public virtual bool ShowAcceptTermsSignUp { get; set; }
        public virtual bool ConfirmUserEmailAddress { get; set; }
        public virtual int PasswordRequiredLength { get; set; } = 6;
        public virtual bool PasswordRequireNonLetterOrDigit { get; set; }
        public virtual bool PasswordRequireDigit { get; set; } = true;
        public virtual bool PasswordRequireLowercase { get; set; }
        public virtual bool PasswordRequireUppercase { get; set; }
        public virtual string SiteEmailAddress { get; set; } = "info@mywebsite.com";
        public virtual string SMTPServer { get; set; }
        public virtual int SMTPPort { get; set; }
        public virtual string SMTPPassword { get; set; }
        public virtual string SMTPUserName { get; set; }
        public virtual bool SMTPEnableSsl { get; set; }

        public virtual bool GetBillingAddressForInvoice { get; set; }
        public virtual bool ShowQtyColInInvoice { get; set; } = true;
        public virtual bool UserMustRegisterBeforeCheckoutInvoice { get; set; } = true;

        public virtual int DownloadUrlsAgeByHour { get; set; } = 48;
        public virtual int NumberOfDownloadUrlsFireLimit { get; set; }
        public virtual string DownloadableFilesExtensions { get; set; } = "*.*";

        public virtual bool Wishlist { get; set; } = true;
        public virtual bool LikePosts { get; set; } = true;
        public virtual bool AppendLanguageCodeToUrl { get; set; } = false;

        public virtual string WebsiteTheme { get; set; } = "Default Theme";
        public virtual bool EnableJsBundling { get; set; } = false;
        public virtual bool EnableHtmlMinification { get; set; } = true;
        public virtual bool EnableResponseCompression { get; set; } = true;
        public virtual bool EnableInlineJsMinification { get; set; } = false;
        public virtual bool EnableCssBundling { get; set; } = false;
        public virtual bool EnableInlineCssMinification { get; set; } = false;
        public virtual bool EnableBlog { get; set; } = true;
        public virtual bool DeleteEmptyInvoices{ get; set; } = true;
        public virtual int DeleteEmptyInvoicesAfterDays{ get; set; } = 5;
        public virtual bool DeletePendingInvoices{ get; set; } = true;
        public virtual int DeletePendingInvoicesAfterDays{ get; set; } = 30;

        public virtual string TinyMCESettings { get; set; } = @"theme: 'silver',
skin: 'oxide',
min_height: 300,
toolbar_drawer: 'floating',
branding: false,
plugins: 'paste wordcount anchor autoresize fullscreen link hr image emoticons imagetools table code quickbars codesample directionality visualchars visualblocks insertdatetime lists advlist template searchreplace media nonbreaking print preview',
toolbar: 'code | undo redo | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify alignnone | styleselect fontsizeselect | ltr rtl | outdent indent blockquote removeformat subscript superscriptcode forecolor backcolor hr bullist numlist image quickimage link unlink',
quickbars_selection_toolbar: 'bold italic | quicklink formatselect blockquote | alignleft aligncenter alignright alignjustify alignnone | ltr rtl | removeformat forecolor backcolor',
quickbars_insert_toolbar: 'quickimage image quicktable link',
contextmenu: 'link image imagetools template table',
codesample_languages: [
    {text: 'HTML/XML', value: 'markup'},
    {text: 'JavaScript', value: 'javascript'},
    {text: 'CSS', value: 'css'},
    {text: 'PHP', value: 'php'},
    {text: 'Ruby', value: 'ruby'},
    {text: 'Python', value: 'python'},
    {text: 'Java', value: 'java'},
    {text: 'JSON', value: 'json'},
    {text: 'C', value: 'c'},
    {text: 'C#', value: 'csharp'},
    {text: 'VB.Net', value: 'vbnet'},
    {text: 'SQL', value: 'sql'},
    {text: 'Pascal', value: 'pascal'},
    {text: 'C++', value: 'cpp'}
],
templates: [
    { title: 'Thank you for contacting us', description: 'Thank you for contacting us', content: '<p><b>Dear User</b> <br/>Thank you for contacting us, <br/><br/>Best Regards,<br/></p>' },
    { title: 'با تشکر از تماس شما', description: 'با تشکر از تماس شما', content: '<p dir=""rtl""><b>کاربر گرامی</b> <br/>از اینکه با ما تماس گرفته اید متشکریم، <br/><br/>با احترام،<br/></p>' },
]";

        //Reminders Settings-------------------------
        public virtual bool SendExpNotificationsForProducts { get; set; } = true;
        public virtual bool SendExpNotificationsForProductsJustOnce { get; set; } = true;
        public virtual int NotificationHourSpanBeforeExpForProducts { get; set; } = 72;
        public virtual int NotificationHourSpanAfterExpForProducts { get; set; } = 72;
        public virtual bool SendExpNotificationsForProductAttributes { get; set; } = false;
        public virtual bool SendExpNotificationsForProductAttributesJustOnce { get; set; } = true;


        public virtual bool SendExpNotificationsForUserPlans { get; set; } = true;
        public virtual bool SendExpNotificationsForUserPlansJustOnce { get; set; } = true;
        public virtual int NotificationHourSpanBeforeExpForUserPlans { get; set; } = 72;
        public virtual int NotificationHourSpanAfterExpForUserPlans { get; set; } = 72;


        public virtual bool SendExpNotificationsForInvoiceManualItems { get; set; } = false;
        public virtual bool SendExpNotificationsForInvoiceManualItemsJustOnce { get; set; } = true;
        public virtual int NotificationHourSpanBeforeExpForInvoiceManualItems { get; set; } = 0;
        public virtual int NotificationHourSpanAfterExpForInvoiceManualItems { get; set; } = 0;
        //-------------------------Reminders Settings
    }
}
