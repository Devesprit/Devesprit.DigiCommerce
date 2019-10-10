using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.Services;
using Devesprit.Services.Currency;
using Devesprit.Services.Localization;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using Newtonsoft.Json;
using Schema.NET;

namespace Devesprit.DigiCommerce.Models.Products
{
    public partial class ProductModel : PostModel
    {
        private CreativeWork _schema;
        public int NumberOfDownloads { get; set; }
        public string NumberOfDownloadsStr => NumberOfDownloads.FormatNumber();
        public double Price { get; set; }
        public double RenewalPrice { get; set; }
        public int PurchaseExpiration { get; set; }
        public TimePeriodType PurchaseExpirationTimeType { get; set; }
        public double PriceForCurrentUser { get; set; }
        public TblUserGroups CurrentUserGroup { get; set; }
        public string FilesPath { get; set; }
        public string DemoFilesPath { get; set; }
        public TblFileServers FileServer { get; set; }
        public TblUserGroups DownloadLimitedToUserGroup { get; set; }
        public bool HigherUserGroupsCanDownload { get; set; }
        public bool AlwaysShowDownloadButton { get; set; }
        public bool CurrentUserHasAlreadyPurchasedThisProduct { get; set; }
        public ProductDownloadModel DownloadModel { get; set; }
        public List<TblProductCheckoutAttributes> CheckoutAttributes { get; set; } =
            new List<TblProductCheckoutAttributes>();

        public override CreativeWork Schema
        {
            get
            {
                if (_schema != null)
                {
                    return _schema;
                }

                var setting = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
                var defaultCurrency = DependencyResolver.Current.GetService<ICurrencyService>().GetDefaultCurrency();
                var images = new List<Uri>();
                foreach (var image in Images)
                {
                    images.Add(new Uri(image.ImageUrl.GetAbsoluteUrl()));
                }

                var result = new WebPage()
                {
                    Name = Title.ConvertHtmlToText(),
                    Headline = Title.ConvertHtmlToText(),
                    Description = Descriptions != null && Descriptions.Count > 0
                        ? JsonConvert.ToString(Descriptions[0].Description.ConvertHtmlToText())
                        : MetaDescription,
                    Keywords = MetaKeyWords,
                    DatePublished = PublishDate,
                    DateCreated = PublishDate,
                    DateModified = LastUpdate,
                    InteractionStatistic = new InteractionCounter()
                    {
                        UserInteractionCount = NumberOfViews,
                    },
                    Url = new Uri(PostUrl.GetAbsoluteUrl()),
                    DiscussionUrl = new Uri(PostUrl.GetAbsoluteUrl()),
                    PrimaryImageOfPage = new ImageObject()
                    {
                        Url = Images != null && Images.Count > 0 ? new Uri(Images[0].ImageUrl.GetAbsoluteUrl()) : null
                    },
                    Image = images,
                    AggregateRating = new AggregateRating()
                    {
                        RatingValue = 5,
                        BestRating = 5,
                        RatingCount = NumberOfLikes + 1,
                    },
                    Publisher = new Organization()
                    {
                        Name = setting.GetLocalized(x => x.SiteName),
                        Email = setting.SiteEmailAddress,
                        Url = new Uri(setting.SiteUrl),
                        Logo = new Uri(setting.GetLocalized(x => x.SiteLogoHeader).GetAbsoluteUrl())
                    },
                    Provider = new Organization()
                    {
                        Name = setting.GetLocalized(x => x.SiteName),
                        Email = setting.SiteEmailAddress,
                        Url = new Uri(setting.SiteUrl),
                        Logo = new Uri(setting.GetLocalized(x => x.SiteLogoHeader).GetAbsoluteUrl())
                    },
                    Offers = new OneOrMany<IOffer>(new Offer()
                    {
                        Price = Price.ToString("F2", CultureInfo.InvariantCulture),
                        PriceCurrency = defaultCurrency.IsoCode
                    }),
                };

                var relatedProducts = DependencyResolver.Current.GetService<ISearchEngine>()
                    .MoreLikeThis(Id, null, 0, PostType.Product, SearchPlace.Title | SearchPlace.Description | SearchPlace.Tags, 20);
                var relatedLinks = new List<Uri>();
                if (!relatedProducts.HasError && relatedProducts.Documents.Count > 0)
                {
                    var posts = DependencyResolver.Current.GetService<IPostService<TblPosts>>().GetItemsById(
                        relatedProducts.Documents.Select(p => p.DocumentId).Take(5)
                            .ToList(), 1,
                        5);

                    foreach (var post in posts)
                    {
                        relatedLinks.Add(
                            new Uri(("/Product/" + post.Slug).GetAbsoluteUrl()));
                    }
                }

                if (relatedLinks.Count > 0)
                {
                    result.RelatedLink = new OneOrMany<Uri>(relatedLinks);
                }


                return result;
            }
            set => _schema = value;
        }
    }
}