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
    }
}