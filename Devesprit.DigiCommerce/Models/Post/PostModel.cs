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
    }
}