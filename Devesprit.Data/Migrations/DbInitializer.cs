using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Xml;
using Devesprit.Data.Domain;
using Devesprit.Data.Properties;

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

            db.SaveChanges();

            ImportLocalizedStrings(db);
        }

        public virtual Dictionary<string, string> Countries { get; } = new Dictionary<string, string>()
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

        public virtual void ImportLocalizedStrings(AppDbContext db)
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

        public virtual void ImportResourcesFromXmlAsync(TblLanguages language, string xml, AppDbContext db)
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
