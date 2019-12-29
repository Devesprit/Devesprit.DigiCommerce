using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Xml;
using Devesprit.Data.Domain;
using Devesprit.Data.Properties;
using Z.EntityFramework.Plus;

namespace Devesprit.Data.Migrations
{
    public partial class DbInitializer : IApplicationDbInitializer
    {
        public virtual void Initialize(AppDbContext db)
        {
            //Default Currency
            TblCurrencies usDollar = null, irRial = null;
            if (!db.Currencies.Any(p => p.IsMainCurrency))
            {
                usDollar = new TblCurrencies()
                {
                    IsoCode = "USD",
                    DisplayOrder = 0,
                    IsMainCurrency = true,
                    Published = true,
                    CurrencyName = "USD (US Dollar)",
                    ExchangeRate = 0,
                    DisplayFormat = "${0:N2}",
                    ShortName = "USD"
                };

                irRial = new TblCurrencies()
                {
                    IsoCode = "IRR",
                    DisplayOrder = 1,
                    IsMainCurrency = false,
                    Published = true,
                    CurrencyName = "IRR (Iranian Rial)",
                    ExchangeRate = 38000,
                    DisplayFormat = "{0:#,###} ریال",
                    ShortName = "Rial"
                };

                db.Currencies.AddOrUpdate(usDollar);
                db.Currencies.AddOrUpdate(irRial);

                db.SaveChanges();
            }
            else
            {
                usDollar = db.Currencies.FirstOrDefault(p => p.IsoCode.ToUpper().Trim() == "USD");
                irRial = db.Currencies.FirstOrDefault(p => p.IsoCode.ToUpper().Trim() == "IRR");
            }

            //Default Language
            TblLanguages enLang, faLang;
            if (!db.Languages.Any(p=> p.IsDefault))
            {
                using (var stream = new MemoryStream())
                {
                    Resources.English_16px.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    enLang = new TblLanguages()
                    {
                        IsoCode = "en",
                        IsRtl = false,
                        DisplayOrder = 0,
                        IsDefault = true,
                        LanguageName = "English",
                        Published = true,
                        Icon = stream.ToArray(),
                        DefaultCurrency = usDollar,
                        DefaultCurrencyId = usDollar?.Id
                    };

                    stream.Seek(0, SeekOrigin.Begin);
                    Resources.Iran_16px.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    faLang = new TblLanguages()
                    {
                        IsoCode = "fa",
                        IsRtl = true,
                        DisplayOrder = 1,
                        IsDefault = false,
                        LanguageName = "فارسی",
                        Published = true,
                        Icon = stream.ToArray(),
                        DefaultCurrency = irRial,
                        DefaultCurrencyId = irRial?.Id
                    };
                }
                
                db.Languages.AddOrUpdate(enLang);
                db.Languages.AddOrUpdate(faLang);
                db.SaveChanges();

                if (irRial != null)
                {
                    db.LocalizedProperty.AddOrUpdate(new TblLocalizedProperty()
                    {
                        EntityId = irRial.Id,
                        LocaleKey = "CurrencyName",
                        LocaleKeyGroup = "Tbl_Currencies",
                        LocaleValue = "ریال ایران",
                        LanguageId = faLang.Id
                    });
                    db.LocalizedProperty.AddOrUpdate(new TblLocalizedProperty()
                    {
                        EntityId = irRial.Id,
                        LocaleKey = "ShortName",
                        LocaleKeyGroup = "Tbl_Currencies",
                        LocaleValue = "ریال",
                        LanguageId = faLang.Id
                    });
                    db.SaveChanges();
                }

                if (usDollar != null)
                {
                    db.LocalizedProperty.AddOrUpdate(new TblLocalizedProperty()
                    {
                        EntityId = usDollar.Id,
                        LocaleKey = "CurrencyName",
                        LocaleKeyGroup = "Tbl_Currencies",
                        LocaleValue = "دلار آمریکا",
                        LanguageId = faLang.Id
                    });
                    db.LocalizedProperty.AddOrUpdate(new TblLocalizedProperty()
                    {
                        EntityId = usDollar.Id,
                        LocaleKey = "ShortName",
                        LocaleKeyGroup = "Tbl_Currencies",
                        LocaleValue = "دلار",
                        LanguageId = faLang.Id
                    });
                    db.SaveChanges();
                }
            }
            else
            {
                faLang = db.Languages.FirstOrDefault(p => p.IsoCode.Trim() == "fa");
            }

            //Countries List
            if (!db.Countries.Any())
            {
                foreach (var country in Countries)
                {
                    var dbCountry = new TblCountries()
                    {
                        CountryName = country.Key
                    };
                    db.Countries.AddOrUpdate(dbCountry);
                    db.SaveChanges();

                    if (faLang != null)
                        db.LocalizedProperty.AddOrUpdate(new TblLocalizedProperty()
                        {
                            EntityId = dbCountry.Id,
                            LanguageId = faLang.Id,
                            LocaleKeyGroup = "Tbl_Countries",
                            LocaleKey = "CountryName",
                            LocaleValue = country.Value
                        });
                }
            }

            //UserRoles
            var adminRole = db.UserRoles.FirstOrDefault(p => p.RoleName == "Administrator");
            if (adminRole == null)
            {
                adminRole = new TblUserRoles()
                {
                    RoleName = "Administrator"
                };
                db.UserRoles.Add(adminRole);
                db.SaveChanges();
            }

            var dbAreaList = db.UserAccessAreas.ToList();
            foreach (var area in UserAccessAreas)
            {
                if (!dbAreaList.Any(p=> p.AreaName == area.AreaName))
                {
                    db.UserAccessAreas.Add(area);
                }
            }
            db.SaveChanges();

            db.UserRolePermissions.Where(p=> p.RoleId == adminRole.Id).Delete();
            db.UserRolePermissions.AddRange(db.UserAccessAreas.ToList().Select(p => new TblUserRolePermissions()
            {
                AreaName = p.AreaName,
                HaveAccess = true,
                RoleId = adminRole.Id
            }));
            db.SaveChanges();
            
            var adminUser = db.Users.FirstOrDefault(p => p.Email == "admin@admin.com");
            if (adminUser != null && adminUser.RoleId == null)
                adminUser.RoleId = adminRole.Id;

            ImportLocalizedStrings(db);
        }

        List<TblUserAccessAreas> UserAccessAreas { get; } = new List<TblUserAccessAreas>()
        {
            new TblUserAccessAreas("", "ManageProducts", "ManageProducts"),
            new TblUserAccessAreas("ManageProducts", "ManageProducts_Add", "Add"),
            new TblUserAccessAreas("ManageProducts", "ManageProducts_Edit", "Edit"),
            new TblUserAccessAreas("ManageProducts", "ManageProducts_Delete", "Delete"),
            new TblUserAccessAreas("ManageProducts", "ManageProductDiscountsForUserGroups", "ManageProductDiscountsForUserGroups"),
            new TblUserAccessAreas("ManageProductDiscountsForUserGroups", "ManageProductDiscountsForUserGroups_Add", "Add"),
            new TblUserAccessAreas("ManageProductDiscountsForUserGroups", "ManageProductDiscountsForUserGroups_Edit", "Edit"),
            new TblUserAccessAreas("ManageProductDiscountsForUserGroups", "ManageProductDiscountsForUserGroups_Delete", "Delete"),
            new TblUserAccessAreas("ManageProducts", "ProductCheckoutAttributes", "ProductCheckoutAttributes"),
            new TblUserAccessAreas("ProductCheckoutAttributes", "ProductCheckoutAttributes_Add", "Add"),
            new TblUserAccessAreas("ProductCheckoutAttributes", "ProductCheckoutAttributes_Edit", "Edit"),
            new TblUserAccessAreas("ProductCheckoutAttributes", "ProductCheckoutAttributes_Delete", "Delete"),
            new TblUserAccessAreas("ProductCheckoutAttributes", "ProductCheckoutAttributes_Options", "Options"),
            new TblUserAccessAreas("ProductCheckoutAttributes_Options", "ProductCheckoutAttributes_Options_Add", "Add"),
            new TblUserAccessAreas("ProductCheckoutAttributes_Options", "ProductCheckoutAttributes_Options_Edit", "Edit"),
            new TblUserAccessAreas("ProductCheckoutAttributes_Options", "ProductCheckoutAttributes_Options_Delete", "Delete"),
            
            new TblUserAccessAreas("", "ManageUserRoles", "ManageUserRoles"),
            new TblUserAccessAreas("ManageUserRoles", "ManageUserRoles_Add", "Add"),
            new TblUserAccessAreas("ManageUserRoles", "ManageUserRoles_Edit", "Edit"),
            new TblUserAccessAreas("ManageUserRoles", "ManageUserRoles_Delete", "Delete"),
            new TblUserAccessAreas("ManageUserRoles", "ManageUserRoles_ApplyPermissions", "ApplyPermissions"),

            new TblUserAccessAreas("", "Reports", "Reports"),
            new TblUserAccessAreas("Reports", "Reports_InvoicesChart", "Invoices"),
            new TblUserAccessAreas("Reports", "Reports_SellsChart", "Selling"),
            new TblUserAccessAreas("Reports", "Reports_UsersChart", "Users"),
            
            new TblUserAccessAreas("", "FileManager", "FileManager"),

            new TblUserAccessAreas("", "ManageBackgroundJobs", "BackgroundJobsManager"),
            new TblUserAccessAreas("ManageBackgroundJobs", "ManageBackgroundJobs_Edit", "Edit"),
            new TblUserAccessAreas("ManageBackgroundJobs", "ManageBackgroundJobs_Delete", "Delete"),
            new TblUserAccessAreas("ManageBackgroundJobs", "ManageBackgroundJobs_PauseResumeJob", "PauseResumeJob"),
            new TblUserAccessAreas("ManageBackgroundJobs", "ManageBackgroundJobs_ExecuteJob", "ExecuteJob"),
            new TblUserAccessAreas("ManageBackgroundJobs", "ManageBackgroundJobs_BackgroundJobServer", "BackgroundJobServer"),

            new TblUserAccessAreas("", "ManageBlogPosts", "ManageBlogPosts"),
            new TblUserAccessAreas("ManageBlogPosts", "ManageBlogPosts_Add", "Add"),
            new TblUserAccessAreas("ManageBlogPosts", "ManageBlogPosts_Edit", "Edit"),
            new TblUserAccessAreas("ManageBlogPosts", "ManageBlogPosts_Delete", "Delete"),

            new TblUserAccessAreas("", "ManageCountries", "ManageCountries"),
            new TblUserAccessAreas("ManageCountries", "ManageCountries_Add", "Add"),
            new TblUserAccessAreas("ManageCountries", "ManageCountries_Edit", "Edit"),
            new TblUserAccessAreas("ManageCountries", "ManageCountries_Delete", "Delete"),

            new TblUserAccessAreas("", "ManageComments", "ManageComments"),
            new TblUserAccessAreas("ManageComments", "ManageComments_PublishUnPublish", "PublishUnPublish"),
            new TblUserAccessAreas("ManageComments", "ManageComments_Edit", "Edit"),
            new TblUserAccessAreas("ManageComments", "ManageComments_Delete", "Delete"),

            new TblUserAccessAreas("", "ManageCurrencies", "ManageCurrencies"),
            new TblUserAccessAreas("ManageCurrencies", "ManageCurrencies_Add", "Add"),
            new TblUserAccessAreas("ManageCurrencies", "ManageCurrencies_Edit", "Edit"),
            new TblUserAccessAreas("ManageCurrencies", "ManageCurrencies_Delete", "Delete"),
            new TblUserAccessAreas("ManageCurrencies", "ManageCurrencies_SetCurrencyAsDefault", "SetAsDefault"),

            new TblUserAccessAreas("", "ManageLanguages", "ManageLanguages"),
            new TblUserAccessAreas("ManageLanguages", "ManageLanguages_Add", "Add"),
            new TblUserAccessAreas("ManageLanguages", "ManageLanguages_Edit", "Edit"),
            new TblUserAccessAreas("ManageLanguages", "ManageLanguages_Delete", "Delete"),
            new TblUserAccessAreas("ManageLanguages", "ManageLanguages_SetLanguageAsDefault", "SetAsDefault"),
            new TblUserAccessAreas("ManageLanguages", "ManageStringResources", "ManageStringResources"),
            new TblUserAccessAreas("ManageStringResources", "ManageStringResources_ExportResources", "ExportStringResources"),
            new TblUserAccessAreas("ManageStringResources", "ManageStringResources_ImportResources", "ImportStringResources"),
            new TblUserAccessAreas("ManageStringResources", "ManageStringResources_Delete", "Delete"),
            new TblUserAccessAreas("ManageStringResources", "ManageStringResources_Edit", "Edit"),
            new TblUserAccessAreas("ManageStringResources", "ManageStringResources_Add", "Add"),

            new TblUserAccessAreas("", "ManageFileServers", "ManageFileServers"),
            new TblUserAccessAreas("ManageFileServers", "ManageFileServers_Add", "Add"),
            new TblUserAccessAreas("ManageFileServers", "ManageFileServers_Edit", "Edit"),
            new TblUserAccessAreas("ManageFileServers", "ManageFileServers_Delete", "Delete"),

            new TblUserAccessAreas("", "SiteSettings", "ManageSettings"),
            new TblUserAccessAreas("SiteSettings", "SiteSettings_ChangeSettings", "ChangeSettings"),
            new TblUserAccessAreas("SiteSettings", "SiteSettings_RefreshSearchEngineIndexes", "RefreshSearchEngineIndexes"),
            new TblUserAccessAreas("SiteSettings", "SiteSettings_PurgeCache", "PurgeCache"),
            new TblUserAccessAreas("SiteSettings", "SiteSettings_ApplicationErrorsLog", "ShowApplicationErrorsLog"),
            new TblUserAccessAreas("SiteSettings_ApplicationErrorsLog", "SiteSettings_ApplicationErrorsLog_Clear", "ClearErrorsLog"),

            new TblUserAccessAreas("", "ManageInvoices", "ManageInvoices"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_Add", "Add"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_Delete", "Delete"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceStatus", "ChangeInvoiceStatus"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoicePaymentDate", "EditInvoicePaymentDate"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceItemExpirationDate", "EditInvoiceItemExpirationDate"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceItemLicenseCode", "EditInvoiceItemLicenseCode"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceUser", "ChangeInvoiceOwner"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceItemUnitPrice", "EditInvoiceItemUnitPrice"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceDiscount", "EditInvoiceDiscount"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceTax", "EditInvoiceTax"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_AddNewItemToInvoiceManually", "AddNewItemToInvoiceManually"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_UpdateInvoiceBillingAddress", "EditInvoiceBillingAddress"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_SetInvoiceNote", "EditInvoiceNote"),
            new TblUserAccessAreas("ManageInvoices", "ManageInvoices_CheckoutInvoice", "CheckoutInvoice"),

            new TblUserAccessAreas("", "ManageNavBar", "ManageNavBar"),
            new TblUserAccessAreas("ManageNavBar", "ManageNavBar_Add", "Add"),
            new TblUserAccessAreas("ManageNavBar", "ManageNavBar_Edit", "Edit"),
            new TblUserAccessAreas("ManageNavBar", "ManageNavBar_Delete", "Delete"),
            new TblUserAccessAreas("ManageNavBar", "ManageNavBar_ChangeOrder", "ChangeOrder"),

            new TblUserAccessAreas("", "ManagePages", "StaticPages"),
            new TblUserAccessAreas("ManagePages", "ManagePages_Add", "Add"),
            new TblUserAccessAreas("ManagePages", "ManagePages_Edit", "Edit"),
            new TblUserAccessAreas("ManagePages", "ManagePages_Delete", "Delete"),

            new TblUserAccessAreas("", "ManageTaxes", "ManageTaxes"),
            new TblUserAccessAreas("ManageTaxes", "ManageTaxes_Add", "Add"),
            new TblUserAccessAreas("ManageTaxes", "ManageTaxes_Edit", "Edit"),
            new TblUserAccessAreas("ManageTaxes", "ManageTaxes_Delete", "Delete"),

            new TblUserAccessAreas("", "ManageUserGroups", "ManageUserGroups"),
            new TblUserAccessAreas("ManageUserGroups", "ManageUserGroups_Add", "Add"),
            new TblUserAccessAreas("ManageUserGroups", "ManageUserGroups_Edit", "Edit"),
            new TblUserAccessAreas("ManageUserGroups", "ManageUserGroups_Delete", "Delete"),

            new TblUserAccessAreas("", "ManageUsers", "ManageUsers"),
            new TblUserAccessAreas("ManageUsers", "ManageUsers_Add", "Add"),
            new TblUserAccessAreas("ManageUsers", "ManageUsers_Edit", "Edit"),
            new TblUserAccessAreas("ManageUsers", "ManageUsers_Delete", "Delete"),

            new TblUserAccessAreas("", "ManagePostTags", "ManagePostTags"),
            new TblUserAccessAreas("ManagePostTags", "ManagePostTags_Add", "Add"),
            new TblUserAccessAreas("ManagePostTags", "ManagePostTags_Edit", "Edit"),
            new TblUserAccessAreas("ManagePostTags", "ManagePostTags_Delete", "Delete"),

            new TblUserAccessAreas("", "ManagePlugins", "ManagePlugins"),
            new TblUserAccessAreas("ManagePlugins", "ManagePlugins_Install", "Install"),
            new TblUserAccessAreas("ManagePlugins", "ManagePlugins_Uninstall", "Uninstall"),
            new TblUserAccessAreas("ManagePlugins", "ManagePlugins_ConfigPlugin", "Configure"),
            new TblUserAccessAreas("ManagePlugins", "ManagePlugins_ReloadList", "Refresh"),

            new TblUserAccessAreas("", "ManagePostImages", "ManageProductsOrBlogPostsImages"),
            new TblUserAccessAreas("ManagePostImages", "ManagePostImages_Add", "Add"),
            new TblUserAccessAreas("ManagePostImages", "ManagePostImages_Edit", "Edit"),
            new TblUserAccessAreas("ManagePostImages", "ManagePostImages_Delete", "Delete"),

            new TblUserAccessAreas("", "ManagePostDescriptions", "ManageProductsOrBlogPostsDescriptions"),
            new TblUserAccessAreas("ManagePostDescriptions", "ManagePostDescriptions_Add", "Add"),
            new TblUserAccessAreas("ManagePostDescriptions", "ManagePostDescriptions_Edit", "Edit"),
            new TblUserAccessAreas("ManagePostDescriptions", "ManagePostDescriptions_Delete", "Delete"),

            new TblUserAccessAreas("", "ManagePostAttributes", "ManagePostAttributes"),
            new TblUserAccessAreas("ManagePostAttributes", "ManagePostAttributes_Add", "Add"),
            new TblUserAccessAreas("ManagePostAttributes", "ManagePostAttributes_Edit", "Edit"),
            new TblUserAccessAreas("ManagePostAttributes", "ManagePostAttributes_Delete", "Delete"),
            new TblUserAccessAreas("ManagePostAttributes", "ManagePostAttributes_Options", "Options"),
            new TblUserAccessAreas("ManagePostAttributes_Options", "ManagePostAttributes_Options_Add", "Add"),
            new TblUserAccessAreas("ManagePostAttributes_Options", "ManagePostAttributes_Options_Edit", "Edit"),
            new TblUserAccessAreas("ManagePostAttributes_Options", "ManagePostAttributes_Options_Delete", "Delete"),
            new TblUserAccessAreas("ManagePostAttributes", "ManagePostAttributes_Mapping", "PostAttributeMapping"),
            new TblUserAccessAreas("ManagePostAttributes_Mapping", "ManagePostAttributes_Mapping_Add", "Add"),
            new TblUserAccessAreas("ManagePostAttributes_Mapping", "ManagePostAttributes_Mapping_Edit", "Edit"),
            new TblUserAccessAreas("ManagePostAttributes_Mapping", "ManagePostAttributes_Mapping_Delete", "Delete"),

            new TblUserAccessAreas("", "ManagePostCategories", "ManagePostCategories"),
            new TblUserAccessAreas("ManagePostCategories", "ManagePostCategories_Add", "Add"),
            new TblUserAccessAreas("ManagePostCategories", "ManagePostCategories_Edit", "Edit"),
            new TblUserAccessAreas("ManagePostCategories", "ManagePostCategories_Delete", "Delete"),
            new TblUserAccessAreas("ManagePostCategories", "ManagePostCategories_ChangeOrder", "ChangeOrder"),
            new TblUserAccessAreas("ManagePostCategories", "ManagePostCategories_GenerateNavbar", "GenerateNavbar"),

            new TblUserAccessAreas("", "ManageRedirects", "ManageUrlRedirects"),
            new TblUserAccessAreas("ManageRedirects", "ManageRedirects_Add", "Add"),
            new TblUserAccessAreas("ManageRedirects", "ManageRedirects_Edit", "Edit"),
            new TblUserAccessAreas("ManageRedirects", "ManageRedirects_Delete", "Delete"),
            new TblUserAccessAreas("ManageRedirects", "ManageRedirects_Post_Add", "AddPostRedirects"),
            new TblUserAccessAreas("ManageRedirects", "ManageRedirects_Post_Edit", "EditPostRedirects"),

            new TblUserAccessAreas("", "ManageUserMessages", "ManageUserMessages"),
            new TblUserAccessAreas("ManageUserMessages", "ManageUserMessages_ReplyToUserMessage", "Reply"),
            new TblUserAccessAreas("ManageUserMessages", "ManageUserMessages_SetMessageStatus", "ReadUnRead"),
            new TblUserAccessAreas("ManageUserMessages", "ManageUserMessages_Delete", "Delete"),

            new TblUserAccessAreas("", "Notifications", "ManageNotifications"),
            new TblUserAccessAreas("Notifications", "Notifications_SendMessageToUser", "SendMessage"),
            new TblUserAccessAreas("Notifications", "Notifications_SetNotificationStatus", "ReadUnRead"),
            new TblUserAccessAreas("Notifications", "Notifications_Delete", "Delete"),

            new TblUserAccessAreas("", "Plugins", "Plugins"),
        };

        Dictionary<string, string> Countries { get; } = new Dictionary<string, string>()
        {
            {"Afghanistan", "افغانستان"},
            {"Albania", "آلبانی"},
            {"Algeria", "الجزایر"},
            {"Andorra", "آندورا"},
            {"Angola", "آنگولا"},
            {"Antigua & Barbuda", "آنتیگا و باربودا"},
            {"Argentina", "آرژانتین"},
            {"Armenia", "ارمنستان"},
            {"Australia", "استرالیا"},
            {"Austria", "اتریش"},
            {"Azerbaijan", "جمهوری آذربایجان"},
            {"The Bahamas", "باهاما"},
            {"Bahrain", "بحرین"},
            {"Bangladesh", "بنگلادش"},
            {"Barbados", "باربادوس"},
            {"Belarus", "بلاروس"},
            {"Belgium", "بلژیک"},
            {"Belize", "بلیز"},
            {"Benin", "بنین"},
            {"Bhutan", "پادشاهی بوتان"},
            {"Bolivia", "بولیوی"},
            {"Bosnia & Herzegovina", "بوسنی و هرزگوین"},
            {"Botswana", "بوتسوانا"},
            {"Brazil", "برزیل"},
            {"Brunei", "برونئی"},
            {"Bulgaria", "بلغارستان"},
            {"Burkina Faso", "بورکینافاسو"},
            {"Burundi", "بوروندی"},
            {"Cambodia", "کامبوج"},
            {"Cameroon", "کامرون"},
            {"Canada", "کانادا"},
            {"Cape Verde", "کیپ ورد"},
            {"Central African Republic", "جمهوری آفریقای مرکزی"},
            {"Chad", "چاد"},
            {"Chile", "شیلی"},
            {"China", "چین"},
            {"Colombia", "کلمبیا"},
            {"Comoros", "کومور"},
            {"Republic of the Congo", "جمهوری کنگو"},
            {"Democratic Republic of the Congo", "جمهوری دموکراتیک کنگو"},
            {"Costa Rica", "کاستاریکا"},
            {"Ivory Coast", "ساحل عاج"},
            {"Croatia", "کرواسی"},
            {"Cuba", "کوبا"},
            {"Cyprus", "قبرس"},
            {"Czech Republic", "جمهوری چک"},
            {"Denmark", "دانمارک"},
            {"Djibouti", "جیبوتی"},
            {"Dominica", "دومینیکا"},
            {"Dominican Republic", "جمهوری دومینیکن"},
            {"East Timor", "تیمور شرقی"},
            {"Ecuador", "اکوادور"},
            {"Egypt", "مصر"},
            {"El Salvador", "السالوادور"},
            {"Equatorial Guinea", "گینه استوایی"},
            {"Eritrea", "اریتره"},
            {"Estonia", "استونی"},
            {"Ethiopia", "اتیوپی"},
            {"Fiji", "فیجی"},
            {"Finland", "فنلاند"},
            {"France", "فرانسه"},
            {"Gabon", "گابون"},
            {"The Gambia", "گامبیا"},
            {"Georgia", "گرجستان"},
            {"Germany", "آلمان"},
            {"Ghana", "غنا"},
            {"Greece", "یونان"},
            {"Grenada", "گرنادا"},
            {"Guatemala", "گواتمالا"},
            {"Guinea", "گینه"},
            {"Guinea Bissau", "گینه بیسائو"},
            {"Guyana", "گویان"},
            {"Haiti", "هائیتی"},
            {"Honduras", "هندوراس"},
            {"Hungary", "مجارستان"},
            {"Iceland", "ایسلند"},
            {"India", "هند"},
            {"Indonesia", "اندونزی"},
            {"Iran", "ایران"},
            {"Iraq", "عراق"},
            {"Ireland", "جمهوری ایرلند"},
            {"Israel", "اسرائیل"},
            {"Italy", "ایتالیا"},
            {"Jamaica", "جامائیکا"},
            {"Japan", "ژاپن"},
            {"Jordan", "اردن"},
            {"Kazakhstan", "قزاقستان"},
            {"Kenya", "کنیا"},
            {"Kiribati", "کیریباتی"},
            {"Democratic People’s Republic of Korea", "کره شمالی"},
            {"Republic of Korea", "کره جنوبی"},
            {"Kuwait", "کویت"},
            {"Kyrgyzstan", "قرقیزستان"},
            {"Laos", "لائوس"},
            {"Latvia", "لتونی"},
            {"Lebanon", "لبنان"},
            {"Lesotho", "لسوتو"},
            {"Liberia", "لیبریا"},
            {"Libya", "لیبی"},
            {"Liechtenstein", "لیختن‌اشتاین"},
            {"Lithuania", "لیتوانی"},
            {"Luxembourg", "لوکزامبورگ"},
            {"Macedonia", "مقدونیه"},
            {"Madagascar", "ماداگاسکار"},
            {"Malawi", "مالاوی"},
            {"Malaysia", "مالزی"},
            {"Maldives", "مالدیو"},
            {"Mali", "مالی"},
            {"Malta", "مالت"},
            {"Marshall Islands", "جزایر مارشال"},
            {"Mauritania", "موریتانی"},
            {"Mauritius", "موریس"},
            {"Mexico", "مکزیک"},
            {"Federated States of Micronesia", "ایالات فدرال میکرونزی"},
            {"Moldova", "مولداوی"},
            {"Monaco", "موناکو"},
            {"Mongolia", "مغولستان"},
            {"Montenegro", "مونته‌نگرو"},
            {"Morocco", "مراکش"},
            {"Mozambique", "موزامبیک"},
            {"Myanmar", "میانمار"},
            {"Namibia", "نامیبیا"},
            {"Nauru", "نائورو"},
            {"Nepal", "نپال"},
            {"Netherlands", "هلند"},
            {"New Zealand", "نیوزیلند"},
            {"Nicaragua", "نیکاراگوئه"},
            {"Niger", "نیجر"},
            {"Nigeria", "نیجریه"},
            {"Norway", "نروژ"},
            {"Oman", "عمان"},
            {"Pakistan", "پاکستان"},
            {"Palau", "پالائو"},
            {"Palestine", "فلسطین"},
            {"Panama", "پاناما"},
            {"Papua New Guinea", "پاپوآ گینه نو"},
            {"Paraguay", "پاراگوئه"},
            {"Peru", "پرو"},
            {"Philippines", "فیلیپین"},
            {"Poland", "لهستان"},
            {"Portugal", "پرتغال"},
            {"Qatar", "قطر"},
            {"Romania", "رومانی"},
            {"Russia", "روسیه"},
            {"Rwanda", "رواندا"},
            {"Saint Kitts and Nevis", "سنت کیتس و نویس"},
            {"Saint Lucia", "سنت لوسیا"},
            {"Saint Vincent and the Grenadines", "سنت وینسنت و گرنادین‌ها"},
            {"Samoa", "ساموآ"},
            {"San Marino", "سن مارینو"},
            {"Sao Tome and Principe", "سائوتومه و پرنسیپ"},
            {"Saudi Arabia", "عربستان سعودی"},
            {"Senegal", "سنگال"},
            {"Serbia", "صربستان"},
            {"Seychelles", "سیشل"},
            {"Sierra Leone", "سیرالئون"},
            {"Singapore", "سنگاپور"},
            {"Slovakia", "اسلواکی"},
            {"Slovenia", "اسلوونی"},
            {"Solomon Islands", "جزایر سلیمان"},
            {"Somalia", "سومالی"},
            {"South Africa", "آفریقای جنوبی"},
            {"South Sudan", "سودان جنوبی"},
            {"Spain", "اسپانیا"},
            {"Sri Lanka", "سری‌لانکا"},
            {"Sudan", "سودان"},
            {"Suriname", "سورینام"},
            {"Swaziland", "سوازیلند"},
            {"Sweden", "سوئد"},
            {"Switzerland", "سوئیس"},
            {"Syria", "سوریه"},
            {"Tajikistan", "تاجیکستان"},
            {"Tanzania", "تانزانیا"},
            {"Thailand", "تایلند"},
            {"Togo", "توگو"},
            {"Tonga", "تونگا"},
            {"Trinidad and Tobago", "ترینیداد و توباگو"},
            {"Tunisia", "تونس"},
            {"Turkey", "ترکیه"},
            {"Turkmenistan", "ترکمنستان"},
            {"Tuvalu", "تووالو"},
            {"Uganda", "اوگاندا"},
            {"Ukraine", "اوکراین"},
            {"United Arab Emirates", "امارات متحده عربی"},
            {"United Kingdom", "بریتانیا"},
            {"United States", "ایالات متحده آمریکا"},
            {"Uruguay", "اروگوئه"},
            {"Uzbekistan", "ازبکستان"},
            {"Vanuatu", "وانواتو"},
            {"Vatican City (Holy See)", "واتیکان"},
            {"Venezuela", "ونزوئلا"},
            {"Vietnam", "ویتنام"},
            {"Yemen", "یمن"},
            {"Zambia", "زامبیا"},
            {"Zimbabwe", "زیمبابوه"},
        };

        void ImportLocalizedStrings(AppDbContext db)
        {
            var enLang = db.Languages.FirstOrDefault(p => p.IsoCode == "en");
            var faLang = db.Languages.FirstOrDefault(p => p.IsoCode == "fa");

            if (!db.LocalizedStrings.Any())
            {
                if (enLang != null)
                {
                    ImportResourcesFromXmlAsync(enLang, Resources.DigiCommerce_StringResources_en, db);
                }

                if (faLang != null)
                {
                    ImportResourcesFromXmlAsync(faLang, Resources.DigiCommerce_StringResources_fa, db);
                }
            }
        }

        void ImportResourcesFromXmlAsync(TblLanguages language, string xml, AppDbContext db)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");
            if (nodes == null)
            {
                return;
            }
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes != null)
                {
                    string name = node.Attributes["Name"]?.InnerText.Trim();
                    string value = "";
                    var valueNode = node.SelectSingleNode("Value");
                    if (valueNode != null)
                        value = valueNode.InnerText;

                    if (String.IsNullOrEmpty(name))
                        continue;

                    
                    db.LocalizedStrings.Add(new TblLocalizedStrings()
                    {
                        LanguageId = language.Id,
                        ResourceName = name,
                        ResourceValue = value
                    });
                }
            }

            db.SaveChanges();
        }
    }
}
