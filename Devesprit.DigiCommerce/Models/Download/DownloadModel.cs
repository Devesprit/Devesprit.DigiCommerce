﻿using System;
using System.Collections.Generic;
using Devesprit.DigiCommerce.Models.Products;
using Devesprit.Services.Products;

namespace Devesprit.DigiCommerce.Models.Download
{
    public partial class DownloadModel
    {
        public string PageTitle { get; set; }
        public UserCanDownloadProductResult? UserHasAccessToFiles { get; set; }
        public string DiscountsForUserGroups { get; set; }
        public string UserGroupName { get; set; }
        public int DownloadLimit { get; set; }
        public string DownloadLimitPer { get; set; }
        public DateTime DownloadLimitResetDate { get; set; }
        public string ProductPageUrl { get; set; }
        public List<FileGroup> FileGroups { get; set; } = new List<FileGroup>();
        public bool IsDemo { get; set; }
        public ProductModel ProductModel { get; set; }
    }
}