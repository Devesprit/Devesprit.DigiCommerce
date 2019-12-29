using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services;
using Devesprit.Services.Localization;
using Devesprit.Services.Posts;
using Devesprit.Services.SearchEngine;
using Devesprit.Utilities;
using Devesprit.Utilities.Extensions;
using Newtonsoft.Json;
using Schema.NET;

namespace Devesprit.DigiCommerce.Models.Post
{
    public partial class PostModel
    {
        private CreativeWork _schema;

        public int Id { get; set; }
        public string Title { get; set; }
        public bool Published { get; set; }
        public bool TitleIsRtl => Title.IsRtlLanguage();
        public int NumberOfViews { get; set; }
        public string NumberOfViewsStr => NumberOfViews.FormatNumber();
        public int NumberOfLikes { get; set; }
        public string NumberOfLikesStr => NumberOfLikes.FormatNumber();
        public DateTime PublishDate { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsFeatured { get; set; }
        public bool ShowSimilarCases { get; set; }
        public bool ShowKeywords { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public string PageTitle { get; set; }
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeyWords { get; set; }
        public LikeWishlistButtonsModel LikeWishlistButtonsModel { get; set; }
        public string PostUrl { get; set; }

        public List<PostCategoriesModel> Categories { get; set; }
        public List<Tuple<int, string>> TagsList { get; set; }
        public List<PostImagesModel> Images { get; set; } = new List<PostImagesModel>();
        public List<PostDescriptionsModel> Descriptions { get; set; } = new List<PostDescriptionsModel>();
        public List<PostAttributesModel> Attributes { get; set; } = new List<PostAttributesModel>();

        public virtual CreativeWork Schema
        {
            get
            {
                if (_schema != null)
                {
                    return _schema;
                }

                var setting = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
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
                    }
                };

                var relatedPosts = DependencyResolver.Current.GetService<ISearchEngine>()
                    .MoreLikeThis(Id, null, 0, PostType.BlogPost, SearchPlace.Title | SearchPlace.Description | SearchPlace.Tags, 20);
                var relatedLinks = new List<Uri>();
                if (!relatedPosts.HasError && relatedPosts.Documents.Count > 0)
                {
                    var posts = DependencyResolver.Current.GetService<IPostService<TblPosts>>().GetItemsById(
                        relatedPosts.Documents.Select(p => p.DocumentId).Take(5)
                            .ToList(), 1,
                        5);

                    foreach (var post in posts)
                    {
                        relatedLinks.Add(
                            new Uri(("/Blog/Post/" + post.Slug).GetAbsoluteUrl()));
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